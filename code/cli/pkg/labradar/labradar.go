// Package labradar holds the things we need to interact with a Labradar, specifically the filesystem.
package labradar

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/util"
	"os"
	"path/filepath"
	"strconv"
)

type DirectoryProviderFn = func() string // TODO [TO20220122] Duplication with list.NewListLbrFilesCmd

//var LBRDirectory string

var (
	lbrDir                                 = EmptyLabradarDirectory
	aferoFs                                = afero.NewOsFs()
	DefaultLabradarDir DirectoryProviderFn = func() string {
		return "LBR" + string(os.PathSeparator)
	}
)

// WithDirectory is used to identify the directory that holds the LBR folder for a device.  A panic will happen if
// the specified directory does not seem to be a valid LBR directory.
func WithDirectory(path string) *Device {
	dir, err := tryParseDirectoryPath(path, aferoFs)
	if err != nil {
		logrus.WithError(err).Panicf("Not a valid LBR directory `%s`.", path)
	}
	lbrDir = dir

	d, e := NewDevice(lbrDir, aferoFs)
	if e != nil {
		logrus.WithError(e).Panicf("Could not get a Device reference for the directory %s.", path)
	}
	return d
}

// SeriesNumber is a custom type that represents a Labradar series.
type SeriesNumber int

func (t *SeriesNumber) String() string {
	return fmt.Sprintf("SR" + util.PadLeft(t.Int(), 4))
}
func (t *SeriesNumber) Int() int64 {
	return int64(*t)
}
func (t *SeriesNumber) ReportCsv() string {
	return fmt.Sprintf("%s Report.csv", t.String())
}
func (t *SeriesNumber) LbrName() string {
	return fmt.Sprintf("%s.lbr", t.String())
}

func (t *SeriesNumber) pathToReportCsvOn(d *Device) string {
	return filepath.Join(d.directory.String(), t.String(), t.ReportCsv())
}

func (t *SeriesNumber) ExistsOn(d *Device) bool {
	exists, err := afero.Exists(d.af, t.pathToReportCsvOn(d))
	if err != nil {
		logrus.WithError(err).Warningf("There was a problem trying determine if the series %s is on the device %s.", t.String(), d.String())
		return false
	}
	return exists
}
func TryParseSeriesNumber(sr string) (SeriesNumber, bool) {
	if len(sr) != 6 {
		return SeriesNumber(0), false
	}
	if sr[0:2] != "SR" {
		return SeriesNumber(0), false
	}

	i, err := strconv.Atoi(sr[2:6])
	if err != nil {
		return 0, false
	}

	return SeriesNumber(i), true
}
func TryParseSRReportCsv(s string) (SeriesNumber, bool) {
	return TryParseSeriesNumber(s[0:6])
}

// SeriesMutatorFn describes a function that can be used to manipulate the values of a Series
type SeriesMutatorFn = func(s *Series)

// JsonFileNameProvider is a function that will return the name of the JSON file for a series.
type JsonFileNameProvider = func() string
type JsonWriter struct {
	afs      afero.Fs
	filename JsonFileNameProvider
}

type Error struct {
	Msg    string
	Number int64
}

func (s Error) Error() string {
	return fmt.Sprintf("There was a problem trying to process series %d: %s.", s.Number, s.Msg)
}

type SeriesNotFoundError struct {
	// The series.Number
	Number SeriesNumber
}

func (e SeriesNotFoundError) Error() string {
	return fmt.Sprintf("%s could not be found.", e.Number.String())
}

func TryParseDeviceId(s string) (DeviceId, bool) {
	const emptyDeviceID = DeviceId("LBR-0000000") // 11 characters

	if len(s) != len(emptyDeviceID) {
		return emptyDeviceID, false
	}

	if s[0:4] != "LBR-" {
		return emptyDeviceID, false
	}

	return DeviceId(s), true

}

type DeviceId string

func (t DeviceId) String() string {
	return string(t)
}

func (t DeviceId) SerialNumber() string {
	return t.String()[4:10]
}
