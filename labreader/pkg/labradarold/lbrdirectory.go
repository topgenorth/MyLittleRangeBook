package labradarold

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"labreader/labradar"
	"path/filepath"
)

// EmptyLabradarDirectory represents an "empty" (null?) directory - one that has not been initialized.
var EmptyLabradarDirectory = Directory("")

// Directory represents a string that is the full path to the LBR directory holding Labradar files.
type Directory string

func (d *Directory) String() string {
	return string(*d)
}

// SeriesNumbers will return a list of SeriesNumbers in the directory.
func (d *Directory) SeriesNumbers(afs afero.Fs) []labradar.SeriesNumber {
	result := make([]labradar.SeriesNumber, 0)
	if *d == EmptyLabradarDirectory {
		return result
	}

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

		exists, _ := afero.Exists(afs, d)
		if exists {
			sn, parsed := TryParseSeriesNumber(s)
			if parsed {
				result = append(result, sn)
			}
		}
	}

	return result
}

//func (d *Directory) LIDFile() string {
//
//	if *d == EmptyLabradarDirectory {
//		logrus.Warnln("An EmptyLabradarDirectory - there is no *.LID file.")
//		return ""
//	}
//
//	fn, err := getLabradarMarkerFile(d.String(), aferoFs)
//	if err != nil {
//		// [TO20220324] This should never happen.
//		logrus.WithError(err).Warningf("Could not file the *.LID file.")
//		return ""
//	}
//
//	return fn
//
//}

//func (d *Directory) DeviceId() DeviceId {
//	filename := filepath.Base(d.LIDFile())
//	serialNumber, err := parseDeviceIdFromFilename(d.LIDFile())
//
//	if err != nil {
//		return "LBR-0000000"
//	}
//
//	return DeviceId(fmt.Sprintf("%s-%s", filename[0:3], serialNumber))
//}

// tryParseDirectoryPath will return a labradarold.Directory if the specified Directory e
func tryParseDirectoryPath(dir string, fs afero.Fs) (Directory, error) {

	if dir == EmptyLabradarDirectory.String() {
		return EmptyLabradarDirectory, fmt.Errorf("EmptyLabradarDirectory!")
	}

	exists, err := afero.DirExists(fs, dir)
	if err != nil {
		return EmptyLabradarDirectory, fmt.Errorf("don't know if the directory %s exists", dir)
	}
	if !exists {
		return EmptyLabradarDirectory, fmt.Errorf("`%s` does not exist", dir)
	}

	isDir, err := afero.IsDir(fs, dir)
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

	_, err = getLabradarMarkerFile(dir, fs)
	if err != nil {
		return "", err
	}

	return Directory(dir), nil
}

// getLabradarMarkerFile will inspect all the files in the LbrDirectory and attempt to find an
// *.LID file that all LBR data folders seem to have.
func getLabradarMarkerFile(path string, af afero.Fs) (string, error) {

	pattern := filepath.Join(path, "LBR*.LID")
	glob, err := afero.Glob(af, pattern)
	if err != nil {
		return "", err
	}

	if glob == nil || len(glob) == 0 {
		return "", fmt.Errorf("Could not find any *.LID files in %s", path)
	}
	if len(glob) != 1 {
		logrus.Debugf("Found multiple LID files - grabbing the first one.")
	}

	lidFile := glob[0]
	if !looksLikeTheLabradarMarkerFile(lidFile) {
		return "", fmt.Errorf("not a valid LID file %s", lidFile)
	}

	f, err := af.Open(lidFile)
	if err != nil {
		return "", fmt.Errorf(" '%s' does not seem to be an LBR directory", path)
	}
	if f == nil {
		return "", fmt.Errorf("does not seem to be a LBR directory '%s'", path)
	}

	return f.Name(), nil
}
