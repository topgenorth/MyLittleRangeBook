package labradar

import (
	"bufio"
	"fmt"
	"github.com/spf13/afero"
)

type CsvFile struct {
	// TODO [TO20211122] Duplication with labrader.LabradarCsvFile
	InputFile      string
	LabradarSeries *Series
	Error          error
}

// ReadFile will try to load the CSV file and store the results in a Series struct.
func ReadFile(c *LabradarCsvFile) *CsvFile {

	filename := c.GetInputFilename()

	file, err := c.FileSystem.Open(filename)
	if err != nil {
		return &CsvFile{
			InputFile:      filename,
			LabradarSeries: nil,
			Error:          err,
		}
	}
	defer closeFile(file)

	sb := NewSeriesBuilder()

	s := bufio.NewScanner(file)
	var lineNumber = 0
	for s.Scan() {
		ld := NewLineOfData(lineNumber, s.Text())
		sb.ParseLine(ld)
		lineNumber++
	}

	r := &CsvFile{
		InputFile:      filename,
		LabradarSeries: sb.Series,
		Error:          nil,
	}

	if err := s.Err(); err != nil {
		r.Error = err
	}

	return r
}

func closeFile(f afero.File) {
	err := f.Close()
	if err != nil {
		fmt.Println(err)
	}
}
