package labradar

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/fs"
	"path/filepath"
)

// EmptyLabradarDirectory represents an "empty" (null?) directory - one that has not be initialized.
var EmptyLabradarDirectory Directory = Directory("")

// Directory represents a string that is the full path to the LBR directory holding Labradar files.
type Directory string

func (d *Directory) String() string {
	return string(*d)
}

// SeriesNumbers will return a list of SeriesNumbers in the directory.
func (d *Directory) SeriesNumbers(afs afero.Fs) []SeriesNumber {
	result := make([]SeriesNumber, 0)

	inputDir := d.String()
	// [TO20220115] Show a list of all the things in the inputDir.
	labradarDir, err := afs.Open(inputDir)
	if err != nil {
		logrus.Warnf("Could not list the contents of %s: %v", inputDir, err)
		return result
	}
	subdirs, err := labradarDir.Readdir(0)
	if err != nil {
		logrus.Warnf("problem finding subdirs in %s: %v", inputDir, err)
		return result
	}

	for _, subdir := range subdirs {
		s := subdir.Name()

		d := filepath.Join(inputDir, s, fmt.Sprintf("%s Report.csv", s))
		if fs.FileExists(d) {
			sn, parsed := TryParseSeriesNumber(s)
			if parsed {
				result = append(result, sn)
			}
		}
	}

	return result
}

// TryParseDirectory will return a labradar.Directory if the specified Directory e
func TryParseDirectory(dir string, af *afero.Afero) (Directory, error) {

	if dir == EmptyLabradarDirectory.String() {
		return EmptyLabradarDirectory, fmt.Errorf("EmptyLabradarDirectory!")
	}

	isDir, err := af.IsDir(dir)
	if err != nil {
		if isDir {
			return EmptyLabradarDirectory, fmt.Errorf("could not determine if '%s' is a directory", dir)
		} else {
			return EmptyLabradarDirectory, fmt.Errorf("%s does not exist", dir)
		}
	}
	if !isDir {
		return EmptyLabradarDirectory, fmt.Errorf("'%s' is not a directory", dir)
	}

	return Directory(dir), nil
}
