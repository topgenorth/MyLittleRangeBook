package labradar

import (
	"bufio"
	"encoding/json"
	"fmt"
	"github.com/carolynvs/aferox"
	"github.com/sirupsen/logrus"
	"opgenorth.net/mylittlerangebook/pkg/context"
	"os"
	"path"
	"path/filepath"
	"strings"
	"time"
)

type Series struct {
	Number     int                 `json:"number"`
	Labradar   *Device             `json:"labradar"`
	Velocities *VelocityData       `json:"velocities"`
	Firearm    *Firearm            `json:"firearm"`
	LoadData   *LoadData           `json:"loadData"`
	Notes      string              `json:"notes"`
	RawData    map[int]*LineOfData `json:"data"`
}

// Filename infer the filename withing the LBR folder.
func (s *Series) Filename() string {

	stub := fmt.Sprintf("%04d", s.Number)
	//goland:noinspection SpellCheckingInspection
	subdir := fmt.Sprintf("SR%s", stub)
	filename := fmt.Sprintf("SR%s Report.csv", stub)
	p := path.Join(subdir, filename)
	return p
}

func NewSeries() *Series {

	now := time.Now()

	u := &UnitsOfMeasure{
		Velocity: "fps",
		Distance: "m",
		Weight:   "gr (grains)",
	}
	d := &Device{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		context.DefaultTimeZone,
		fmt.Sprintf("SR%04d", 0),
		u,
	}

	vd := &VelocityData{
		Average:           0,
		Max:               0,
		Min:               0,
		ExtremeSpread:     0,
		StandardDeviation: 0,
		Values:            nil,
	}
	f := &Firearm{
		Name:      "",
		Cartridge: "",
	}
	pr := &Projectile{
		Name:   "",
		Weight: 0,
		BC: &BallisticCoefficient{
			DragModel: "",
			Value:     0,
		},
	}
	po := &PowderCharge{
		Name:   "",
		Amount: 0,
	}
	ld := &LoadData{
		Cartridge:  "",
		Projectile: pr,
		Powder:     po,
	}
	ls := &Series{
		Number:     0,
		Labradar:   d,
		Velocities: vd,
		Firearm:    f,
		LoadData:   ld,
		Notes:      "",
		RawData:    make(map[int]*LineOfData),
	}

	return ls
}

func (s Series) TotalNumberOfShots() int {
	return len(s.Velocities.Values)
}

func (s Series) ToJsonBytes() []byte {
	jsonBytes, err := json.MarshalIndent(SortLinesOfData(s.RawData), "", "  ")
	if err != nil {
		return nil
	}

	return jsonBytes
}

func (s Series) ToJson() (string, error) {
	jsonBytes, err := json.MarshalIndent(SortLinesOfData(s.RawData), "", "  ")
	if err != nil {
		return "", err
	}

	return fmt.Sprintf("%x", jsonBytes), nil
}

func (s *Series) SetProjectile(projectileDescription string) {
	if len(projectileDescription) > 0 {
		s.LoadData.Projectile = parseProjectileString(projectileDescription)
	}
}

func (s *Series) SetPowder(powderDescription string) {
	if len(powderDescription) > 0 {
		s.LoadData.Powder = parsePowderString(powderDescription)
	}
}

func initDevice(seriesNumber int, timezone *time.Location) *Device {
	now := time.Now().In(timezone)

	u := &UnitsOfMeasure{
		Velocity: "fps",
		Distance: "m",
		Weight:   "gr (grains)",
	}
	return &Device{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		timezone.String(),
		fmt.Sprintf("SR%04d", seriesNumber),
		u,
	}
}

//// LoadCsv will read a single Labradar CSV file (identified by it's seriesNumber).
//func LoadCsv(c *config.Config, inputDir string, seriesNumber int) *CsvFile {
//	name := fs.FilenameForSeries(inputDir, seriesNumber)
//	series, err := LoadSeries(name, c.FileSystem)
//	if err != nil {
//		return &CsvFile{Series: nil, InputFile: name, Error: err}
//	}
//
//	return &CsvFile{Series: series, InputFile: name, Error: nil}
//}

// loadCsvInternal Will deserialize a Labaradar file into a CsvFile.
//func loadCsvInternal(fs aferox.Aferox, filename string) *CsvFile {
//	f, err := fs.Open(filename)
//	if err != nil {
//		return &CsvFile{
//			nil,
//			filename,
//			err,
//		}
//	}
//	defer fs.CloseFile(f)
//
//	sb := NewSeriesBuilder()
//	s := bufio.NewScanner(f)
//	var lineNumber = 0
//	for s.Scan() {
//		ld := NewLineOfData(lineNumber, s.Text())
//		sb.ParseLine(ld)
//		lineNumber++
//	}
//
//	return &CsvFile{
//		Series:    sb.Series,
//		InputFile: filename,
//		Error:     nil,
//	}
//}

//func outputFileNameFor(seriesNumber int, outputDir string) string {
//	stub := fmt.Sprintf("%04d", seriesNumber)
//	filename := fmt.Sprintf("%s.json", stub)
//	return path.Join(outputDir, filename)
//}

// isLabradarCsvFile will perform some basic checking to see if a given filename could be that of a Labadar CSV file.
func isLabradarCsvFile(path string) bool {
	b := strings.ToLower(filepath.Base(path))
	ext := filepath.Ext(b)
	if ext == ".csv" {
		sr := b[0:2]
		if sr == "sr" {
			return true
		}
	}

	return false
}

// getCsvFilenamesInDirectory will return a list of Labradar CSV files in a given directory
func getCsvFilenamesInDirectory(inputDir string) []string {

	var filenames []string

	// TODO [TO20211122] Can't really mock out filepath.Walk.
	err := filepath.Walk(inputDir, func(path string, info os.FileInfo, err error) error {
		if !info.IsDir() && isLabradarCsvFile(path) {
			filenames = append(filenames, path)
		}
		return nil
	})
	if err != nil {
		logrus.Errorf("Could not list files in the directory %s. %v", inputDir, err)
		return nil
	}

	return filenames
}

// LoadSeries will take the specified CSV file and return a Series.
func LoadSeries(filename string, fs aferox.Aferox) (*Series, error) {
	f, err := fs.Open(filename)
	if err != nil {
		return nil, fmt.Errorf("could not load the series at %s: %w", filename, err)
	}

	//defer CloseFile(f)

	builder := NewSeriesBuilder()
	scanner := bufio.NewScanner(f)
	var lineNumber = 0
	for scanner.Scan() {
		l := NewLineOfData(lineNumber, scanner.Text())
		builder.ParseLine(l)
		lineNumber++
	}

	return builder.Series, nil
}
