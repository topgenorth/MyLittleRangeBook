// Package labradar holds the things we need to interact with a Labradar, specifically the filesystem.
package labradar

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"os"
)

type DirectoryProviderFn = func() string // TODO [TO20220122] Duplication with list.NewListLbrFilesCmd

var (
	lbrDir             = EmptyLabradarDirectory
	aferoFs            = afero.NewOsFs()
	DefaultLabradarDir = func() string {
		return "LBR" + string(os.PathSeparator)
	}
)

// WithDirectory is used to identify the directory that holds the LBR folder for a device.  A panic will happen if
// the specified directory does not seem to be a valid LBR directory.
func WithDirectory(path string) *DeviceDirectory {
	dir, err := tryParseDirectoryPath(path, aferoFs)
	if err != nil {
		logrus.WithError(err).Panicf("Not a valid LBR directory `%s`.", path)
	}
	lbrDir = dir

	d, e := NewDeviceDirectory(lbrDir, aferoFs)
	if e != nil {
		logrus.WithError(e).Panicf("Could not get a DeviceDirectory reference for the directory %s.", path)
	}
	return d
}

func TryParseSRReportCsv(s string) (SeriesNumber, bool) {
	return TryParseSeriesNumber(s[0:6])
}

// JsonFileNameProvider is a function that will return the name of the JSON file for a series.
type JsonFileNameProvider = func() string
type JsonWriter struct {
	afs      afero.Fs
	filename JsonFileNameProvider
}
