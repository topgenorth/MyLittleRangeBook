package labradar

// FileDirectory represents a string that is the full path to the LBR directory holding Labradar files.
type FileDirectory string

// EmptyLabradarDirectory represents an "empty" (null?) directory - one that has not been initialized.
var EmptyLabradarDirectory = FileDirectory("")

func (d *FileDirectory) String() string {
	return string(*d)
}
