package labradar

import (
	"bufio"
	"fmt"
	"github.com/spf13/afero"
	"path/filepath"
	"strconv"
	"strings"
	"time"
)

// CsvFile is a serialized Labradar CSV file.
type CsvFile struct {
	InputFile string
	Error     error
	lines     []string
}

// String will return the name of a CsvFile.
func (f CsvFile) String() string {
	return f.InputFile
}

// loadCsv will read a file line at a time, capturing each line of test for future processing.
func loadCsv(filename string, fs *afero.Afero) *CsvFile {
	csv := &CsvFile{
		InputFile: filename,
		Error:     nil,
		lines:     nil,
	}

	f, err := fs.Open(filename)
	if err != nil {
		csv.Error = fmt.Errorf("could not load the series at %s: %w", filename, err)
		return csv
	}
	defer CloseFile(f)

	scanner := bufio.NewScanner(f)
	for scanner.Scan() {
		line := scanner.Text()
		csv.lines = append(csv.lines, line)
	}

	return csv
}

// withSeriesDateFromCsv will extract the date & time from the CSV.
func withSeriesDateFromCsv(file CsvFile) SeriesMutatorFn {
	return func(s *Series) {
		d, t := getDateAndTimeStrings(file, 18)
		s.Date = d
		s.Time = t
	}
}

// withDeviceIdFromCsv will extract the device ID from within the CSV file.
func withDeviceIdFromCsv(file CsvFile) SeriesMutatorFn {
	return func(s *Series) {
		did := getStringValue(file, 1)
		s.deviceId = DeviceId(did)
	}
}

// withSeriesNumberFromCsv will extract the device ID from the CSV file.
func withSeriesNumberFromCsv(file CsvFile) SeriesMutatorFn {
	return func(s *Series) {
		s.Number = SeriesNumber(getIntValue(file, 3))
	}
}

// withUnitsOfMeasureFromCsv will extract the units of measure from the CSV file.
func withUnitsOfMeasureFromCsv(file CsvFile) SeriesMutatorFn {
	return func(s *Series) {
		s.UnitsOfMeasure.Velocity = getStringValue(file, 6)
		s.UnitsOfMeasure.Distance = getStringValue(file, 7)
		s.UnitsOfMeasure.Weight = getStringValue(file, 9)
	}
}

// addMuzzleVelocitiesFromCsv will read all the muzzle velocities from the CSV file.
func addMuzzleVelocitiesFromCsv(file CsvFile) SeriesMutatorFn {
	return func(s *Series) {
		for i := 18; i < len(file.lines); i++ {
			velocity := getIntValue(file, i)
			s.Velocities.Values = append(s.Velocities.Values, velocity)
		}
	}
}

// getDateAndTimeStrings will attempt to parse the date & time from a line in a CSV file.
func getDateAndTimeStrings(file CsvFile, lineNumber int) (string, string) {

	line := SanitizeLine(file.lines[lineNumber])
	parts := strings.Split(line, ";")
	x := len(parts)
	if x == 1 {
		return "", ""
	}

	d := parts[x-3]
	t := parts[x-2]
	//goland:noinspection SpellCheckingInspection
	mydate, _ := time.Parse("01-02-2006 15:04:05", d+" "+t)

	return mydate.Format("2006-Jan-02"), mydate.Format("15:04")
}

// getIntValue will attempt to parse an integer value from a line in the CSV file.
func getIntValue(file CsvFile, lineNumber int) int {

	parts := strings.Split(SanitizeLine(file.lines[lineNumber]), ";")
	i, err := strconv.Atoi(parts[1])
	if err != nil {
		return -1
	}
	return i
}

// getStringValue will attempt to parse a string value from a line in the CSV file.
func getStringValue(file CsvFile, lineNumber int) string {
	parts := strings.Split(SanitizeLine(file.lines[lineNumber]), ";")
	return parts[1]
}

// IsLabradarCsvFile will perform some basic checking to see if a given filename could be that of a Labradar CSV file.
// This is a case sensitive comparison.  Sample name: SR0001 Report.csv
func IsLabradarCsvFile(filenameWithExtension string) bool {

	if len(filenameWithExtension) != 17 {
		return false
	}

	b := filepath.Base(filenameWithExtension)
	ext := filepath.Ext(b)

	// Check extension
	if ext != ".csv" {
		return false
	}

	name := strings.Split(b, ".")[0]

	// Check length of the filename
	if len(name) != 13 {
		return false
	}

	// Must end with ` Report`
	if !strings.HasSuffix(name, " Report") {
		return false
	}

	// Must start with a valid Labradar OldSeries name
	_, parsed := TryParseSRReportCsv(filenameWithExtension)

	return parsed

}
