package labradarold

/*// Package labradarold holds the things we need to interact with a Labradar, specifically the filesystem.


import (
	"labreader/labradar"
	"os"
)

// WithDirectory is used to identify the directory that holds the LBR folder for a device.  A panic will happen if
// the specified directory does not seem to be a valid LBR directory.
//func WithDirectory(path string) *DeviceDirectory {
//	dir, err := tryParseDirectoryPath(path, aferoFs)
//	if err != nil {
//		logrus.WithError(err).Panicf("Not a valid LBR directory `%s`.", path)
//	}
//	lbrDir = dir
//
//	d, e := NewDeviceDirectory(lbrDir, aferoFs)
//	if e != nil {
//		logrus.WithError(e).Panicf("Could not get a DeviceDirectory reference for the directory %s.", path)
//	}
//	return d
//}

var (
	lbrDir = EmptyLabradarDirectory
	//aferoFs            = afero.NewOsFs()
	DefaultLabradarDir = func() string {
		return "LBR" + string(os.PathSeparator)
	}
)

type DirectoryProviderFn = func() string // TODO [TO20220122] Duplication with list.NewListLbrFilesCmd

func TryParseSRReportCsv(s string) (labradar.SeriesNumber, bool) {
	return TryParseSeriesNumber(s[0:6])
}

type SeriesTemplateType string

const (
	SimplePlainText      SeriesTemplateType = "SimplePlainText"
	DescriptivePlainText SeriesTemplateType = "DescriptivePlainText"
	JSON                 SeriesTemplateType = "Json"
)

//
//// JsonFileNameProvider is a function that will return the name of the JSON file for a series.
//type JsonFileNameProvider = func() string
//type JsonWriter struct {
//	afs      afero.Fs
//	filename JsonFileNameProvider
//}
*/
