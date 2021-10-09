package labradar

import (
	"bufio"
)

type CsvConversion struct {
	InputFile      string
	LabradarSeries *Series
	Error          error
}

// NewCsvConversion will try to load the CSV file and store the results in a Series struct.
func NewCsvConversion(c *ReadCsvConfig) *CsvConversion {
	r := &CsvConversion{
		InputFile:      c.GetInputFilename(),
		LabradarSeries: NewSeries(c.SeriesNumber, c.TimeLocation()),
		Error:          nil,
	}

	file, err := openFile(r.InputFile, c.Filesystem)
	if err != nil {
		r.Error = err
	}
	defer closeFile(file)

	s := bufio.NewScanner(file)
	var lineNumber = 0
	for s.Scan() {
		lineOfData := newLineOfData(lineNumber, s.Text())
		r.LabradarSeries.parseLineOfTextFromLabradarCsv(lineOfData)
		lineNumber++
	}

	if err := s.Err(); err != nil {
		r.Error = err
	}

	return r
}

