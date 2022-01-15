package fs

// CsvFile is a serialized Labradar CSV file.
type CsvFile struct {
	contents  []byte
	InputFile string
	Error     error
}

func (f CsvFile) String() string {
	return f.InputFile
}
