package labradar

import (
	"bufio"
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/labradar/pkg"
	"opgenorth.net/labradar/pkg/config"
	"os"
	"path/filepath"
	"strings"
	"time"
)

func NewSeries(seriesNumber int, cfg *config.Config) *Series {

	ls := &Series{
		Number:   seriesNumber,
		Labradar: initLabradarStruct(seriesNumber, cfg.TimeZone),
		Firearm: &Firearm{
			Name:      "",
			Cartridge: "",
		},
		LoadData: &LoadData{
			Cartridge: "",
			Projectile: &Projectile{
				Name:   "",
				Weight: 0,
				BC: &BallisticCoefficient{
					DragModel: "",
					Value:     0,
				},
			},
			Powder: &PowderCharge{
				Name:   "",
				Amount: 0,
			},
		},
		Notes:   "",
		Tags:    make([]string, 10), // making a wild guess at how many tags we'll need.
		RawData: make(map[int]*LineOfData),
	}

	return ls
}

func closeFile(f afero.File) {
	err := f.Close()
	if err != nil {
		fmt.Println(err)
	}
}
func openFile(filename string, a afero.Afero) (afero.File, error) {
	file, err := a.Open(filename)
	if err != nil {
		return nil, err
	}

	return file, nil
}

func LoadLabradarSeriesFromCsv(ls *Series, cfg *config.Config) error {
	inputFileName := filepath.Join(cfg.InputDir, ls.Labradar.SeriesName, ls.Labradar.SeriesName+" Report.csv")

	file, err := openFile(inputFileName, cfg.Context.Afero)
	if err != nil {
		fmt.Println("Could not open the file " + inputFileName)
		return err
	}
	defer closeFile(file)

	s := bufio.NewScanner(file)
	var lineNumber = 0
	for s.Scan() {
		lineOfData := CreateLine(lineNumber, s.Text())
		ls.parseLineOfTextFromLabradarCsv(lineOfData)
		lineNumber++
	}

	if err := s.Err(); err != nil {
		return err
	}

	return nil
}

func SaveLabradarSeriesToJson(ls *Series, cfg *config.Config) error {
	outputFileName := filepath.Join(cfg.OutputDir, ls.Labradar.SeriesName+".json")

	err := deleteFileIfExists(cfg.Context.Afero, outputFileName)
	if err != nil {
		return err
	}

	err2 := cfg.Context.Afero.WriteFile(outputFileName, ls.ToJson(), 0644)
	if err2 != nil {
		return err2
	}

	return nil
}

func deleteFileIfExists(a afero.Afero, fileName string) error {
	exists, _ := a.Exists(fileName)
	if exists {
		err := os.Remove(fileName)
		if err != nil {
			return err
		}
	}
	return nil
}

func fixupLabradarLine(line string) string {
	parts := strings.Split(strings.TrimSpace(line), pkg.UnicodeNUL)

	switch lengthOfParts := len(parts); lengthOfParts {
	case 2:
		// The string either started with a NUL or ended with a NUL
		if len(parts[0]) == 0 {
			return parts[1]
		}
		if len(parts[1]) == 0 {
			return parts[0]
		}

		return line
	case 3:
		// The string started with NUL and and ended with NUL
		return parts[1]
	default:

		return line
	}

}

func (ls *Series) parseLineOfTextFromLabradarCsv(ld *LineOfData) {
	switch ld.LineNumber {
	case 1:
		ls.RawData[ld.LineNumber] = ld
		ls.Labradar.DeviceId = ld.GetString()
	case 3:
		ls.RawData[ld.LineNumber] = ld
	case 6:
		ls.RawData[ld.LineNumber] = ld
		ls.Labradar.Units.Velocity = ld.GetString()
	case 7:
		ls.RawData[ld.LineNumber] = ld
		ls.Labradar.Units.Distance = ld.GetString()
	case 9:
		ls.RawData[ld.LineNumber] = ld
		ls.Labradar.Units.Weight = ld.GetString()
	case 18:
		// For now, we only care about V0 (i.e. the muzzle velocity).
		ls.RawData[ld.LineNumber] = ld
		ls.Labradar.Stats.AddVelocity(ld.GetInt())

		// We also pull the date and time from the first shot recorded
		ls.Labradar.Date, ls.Labradar.Time = ld.GetDateAndTime()

	default:
		if ld.LineNumber > 18 {
			ls.RawData[ld.LineNumber] = ld
			ls.Labradar.Stats.AddVelocity(ld.GetInt())
		}
	}
}

func initLabradarStruct(seriesNumber int, timezone *time.Location) *Device {
	now := time.Now().In(timezone)

	return &Device{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		timezone.String(),
		fmt.Sprintf("SR%04d", seriesNumber),
		&UnitsOfMeasure{
			Velocity: "fps",
			Distance: "m",
			Weight:   "gr (grains)",
		},
		&Velocities{
			Average:            0,
			Max:                0,
			Min:                0,
			ExtremeSpread:      0,
			StandardDeviation:  0,
			VelocitiesInSeries: nil,
		},
	}
}
