package fs

import (
	"bufio"
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
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

// GetMutatorsToUpdateSeries will return an array of mutators to update a series.LabradarSeries with the contents of a
// CSV file.
func GetMutatorsToUpdateSeries(seriesNumber int, directory string, af *afero.Afero) ([]series.LabradarSeriesMutatorFunc, error) {
	filename := FilenameForSeries(directory, seriesNumber)
	exists, err := af.Exists(filename)
	if err != nil {
		return make([]series.LabradarSeriesMutatorFunc, 0), fmt.Errorf("could not load the file '%s': %w", filename, err)
	}
	if !exists {
		return make([]series.LabradarSeriesMutatorFunc, 0), fmt.Errorf("could not load the file '%s'", filename)
	}

	// [TO20220118] We dereference file - the thought being that this could allow us to parallelize this? Probably
	// a case of premature optimization though.
	file := *loadCsv(filename, af)
	return []series.LabradarSeriesMutatorFunc{
		withDeviceIdFrom(file),
		withSeriesNumberFrom(file),
		withUnitsOfMeasureFrom(file),
		addMuzzleVelocitiesFrom(file),
		withSeriesDateFrom(file),
	}, nil

}

// withSeriesDateFrom will extract the date & time from the CSV.
func withSeriesDateFrom(file CsvFile) series.LabradarSeriesMutatorFunc {
	return func(s *series.LabradarSeries) {
		d, t := getDateAndTimeStrings(file, 18)
		s.Date = d
		s.Time = t
	}
}

// withDeviceIdFrom will extract the device ID from within the CSV file.
func withDeviceIdFrom(file CsvFile) series.LabradarSeriesMutatorFunc {
	return func(s *series.LabradarSeries) {
		s.DeviceId = getStringValue(file, 1)
	}
}

// withSeriesNumberFrom will extract the device ID from the CSV file.
func withSeriesNumberFrom(file CsvFile) series.LabradarSeriesMutatorFunc {
	return func(s *series.LabradarSeries) {
		s.Number = getIntValue(file, 3)
	}
}

// withUnitsOfMeasureFrom will extract the units of measure from the CSV file.
func withUnitsOfMeasureFrom(file CsvFile) series.LabradarSeriesMutatorFunc {
	return func(s *series.LabradarSeries) {
		s.UnitsOfMeasure.Velocity = getStringValue(file, 6)
		s.UnitsOfMeasure.Distance = getStringValue(file, 7)
		s.UnitsOfMeasure.Weight = getStringValue(file, 9)
	}
}

// addMuzzleVelocitiesFrom will read all the muzzle velocities from the CSV file.
func addMuzzleVelocitiesFrom(file CsvFile) series.LabradarSeriesMutatorFunc {
	return func(s *series.LabradarSeries) {
		for i := 18; i < len(file.lines); i++ {
			velocity := getIntValue(file, i)
			s.Velocities.Values = append(s.Velocities.Values, velocity)
		}
	}
}

// getDateAndTimeStrings will attempt to parse the date & time from a line in a CSV file.
func getDateAndTimeStrings(file CsvFile, lineNumber int) (string, string) {
	parts := strings.Split(sanitizeCsvLine(file, lineNumber), ";")
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
	parts := strings.Split(sanitizeCsvLine(file, lineNumber), ";")
	i, err := strconv.Atoi(parts[1])
	if err != nil {
		return -1
	}
	return i
}

// getStringValue will attempt to parse a string value from a line in the CSV file.
func getStringValue(file CsvFile, lineNumber int) string {
	parts := strings.Split(sanitizeCsvLine(file, lineNumber), ";")
	return parts[1]
}
