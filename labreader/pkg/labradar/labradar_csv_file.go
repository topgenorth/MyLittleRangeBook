package labradar

import (
	"bufio"
	"github.com/pkg/errors"
	"github.com/spf13/afero"
	constants "labreader/internal"
	"math"
	"strconv"
	"strings"
)

const (
	lineNumberSeparator        = 0
	lineNumberDeviceId         = 1
	LineNumberSeries           = 3
	lineNumberUnitsVelocity    = 6
	lineNumberUnitsDistance    = 7
	lineNumberUnitWeight       = 9
	lineNumberBeforeVelocities = 17
	LineNumberVelocityStart    = 18
)

type CsvFileContents struct {
	inputFilename string
	Err           error
	Lines         []string
}

// LoadCsv will read the lines of the file, one at a time.
func LoadCsv(filename string, fs *afero.Afero) *CsvFileContents {
	csv := &CsvFileContents{
		inputFilename: filename,
		Err:           nil,
		Lines:         nil,
	}

	if _, err := fs.Stat(filename); err != nil {
		csv.Err = err
		return csv
	}

	f, err := fs.Open(filename)
	if err != nil {
		csv.Err = errors.Wrap(err, "could not open the file")
		return csv
	}
	defer func(f afero.File) {
		_ = f.Close()
	}(f)

	sc := bufio.NewScanner(f)
	for sc.Scan() {
		line := sc.Text()
		csv.Lines = append(csv.Lines, line)
	}
	return csv
}

func sanitizeLineFromLabradarCSV(line string) string {
	parts := strings.Split(strings.TrimSpace(line), constants.UnicodeNUL)

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
		// The string started with NUL and ended with NUL
		return parts[1]
	default:
		return line
	}
}

// GetIntValue will attempt to parse an integer value from a line in the CSV file.
func (csv *CsvFileContents) GetIntValue(lineNumber int) int {
	line := sanitizeLineFromLabradarCSV(csv.Lines[lineNumber])
	parts := strings.Split(line, ";")

	v, err := strconv.ParseFloat(parts[1], 64)
	if err != nil {
		return -1
	}
	return int(math.Round(v))

}
