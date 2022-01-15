package io

import (
	"github.com/carolynvs/aferox"
)

// CsvFile is a serialized Labradar CSV file.
type CsvFile struct {
	contents  []byte
	InputFile string
	Error     error
}

func (f CsvFile) String() string {
	return f.InputFile
}

func LoadCsv(filename string, fs aferox.Aferox) *CsvFile {
	csv := &CsvFile{
		contents:  make([]byte, 0),
		InputFile: filename,
		Error:     nil,
	}

	f, err := fs.Open(filename)
	defer CloseFile(f)
	if err != nil {
		csv.Error = err
		return csv
	}
	fileinfo, err := f.Stat()
	if err != nil {
		csv.Error = err
		return csv
	}

	filesize := int(fileinfo.Size())
	csv.contents = make([]byte, filesize)

	_, err = f.Read(csv.contents)
	if err != nil {
		csv.Error = err
	}

	return csv
}
