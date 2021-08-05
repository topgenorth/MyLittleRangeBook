package labradar

import (
	"fmt"
	"opgenorth.net/labradar/fs"
	"os"
	"path/filepath"
	"strings"
	"time"
)

const DirectoryForJson = "/Users/tom/work/topgenorth.github.io/data/labradar/"

func fixupLabradarLine(line string) string {
	parts := strings.Split(strings.TrimSpace(line), fs.UnicodeNUL)

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

func (ls *Series) parseLine(ld *LineOfData) {
	switch ld.LineNumber {
	case 1:
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.DeviceId = ld.GetString()
	case 3:
		ls.Data[ld.LineNumber] = ld
		ls.Number = ld.GetInt()
		ls.Labradar.SeriesName = formatLabradarSeriesNumber(ls.Number)
	case 6:
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.Units.Velocity = ld.GetString()
	case 7:
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.Units.Distance = ld.GetString()
	case 9:
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.Units.Weight = ld.GetString()
	case 18:
		// For now, we only care about V0 (i.e. the muzzle velocity).
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.Stats.AddVelocity(ld.GetInt())

		// We also pull the date and time from the first shot recorded
		ls.Labradar.Date, ls.Labradar.Time = ld.GetDateAndTime()

	default:
		if ld.LineNumber > 18 {
			ls.Data[ld.LineNumber] = ld
			ls.Labradar.Stats.AddVelocity(ld.GetInt())
		}
	}
}

func (ls *Series) jsonFileName(directory string) string {
	filename := filepath.Join(directory, ls.Labradar.SeriesName+".json")
	return filename
}

func getPathToLabradarCsvFile(seriesNumber int) (string, error) {
	type fileParts struct {
		InputNameParts []string
		PathSep        string
		HomeDir        string
		LbrToken       string
	}

	homeDir, err := os.UserHomeDir()
	if err != nil {
		return "", err
	}

	var parts = &fileParts{
		[]string{"work", "labradar", "LBR"},
		string(os.PathSeparator),
		homeDir,
		fmt.Sprintf("SR%04d", seriesNumber),
	}
	var pathToSeries = parts.HomeDir + parts.PathSep
	for _, part := range parts.InputNameParts {
		pathToSeries += part
		pathToSeries += parts.PathSep
	}
	pathToSeries += parts.LbrToken + parts.PathSep + parts.LbrToken + " Report.csv"
	return pathToSeries, nil

}

func formatLabradarSeriesNumber(seriesNumber int) string {
	return fmt.Sprintf("SR%04d", seriesNumber)
}

func initLabradarStruct(seriesNumber int) *Labradar {
	loc, _ := time.LoadLocation("UTC")
	now := time.Now().In(loc)

	return &Labradar{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		formatLabradarSeriesNumber(seriesNumber),
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
