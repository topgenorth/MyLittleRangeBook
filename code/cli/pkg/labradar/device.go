package labradar

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/util"
	"path/filepath"
	"strings"
)

// NewDevice will initialize a Device structure. The path parameter is the path to the LBR folder
// of the Labradar device.
func NewDevice(lbrDirectory Directory, fs afero.Fs) (*Device, error) {

	f, err := lbrDirectory.getLabradarMarkerFile(fs)
	if err != nil {
		return nil, err
	}

	return &Device{
		deviceId:  getDeviceId(f.Name()),
		timeZone:  "America/Edmonton",
		directory: lbrDirectory,
		af:        &afero.Afero{Fs: fs},
	}, nil
}

func (d *Device) DeviceId() DeviceId {
	return d.deviceId
}

func (d *Device) TimeZone() string {
	return d.timeZone
}

func (d *Device) Directory() Directory {
	return d.directory
}

func (d Device) String() string {
	return d.deviceId.String()
}

// Series will retrieve the specified series from the Labradar Device.
func (d Device) Series(n SeriesNumber) (*Series, error) {
	directory := d.Directory()
	csvValues, err := updateSeriesFromCsvFile(n, directory.String(), d.af)

	if err != nil {
		e := Error{
			Msg:    fmt.Sprintf("could not load the series %d from  device %s (%s): %v", n, d.DeviceId(), d.Directory(), err),
			Number: n.Int(),
		}
		return nil, e
	}

	mutators := combineMutators(LabradarSeriesDefaults(), csvValues)
	s := NewSeries(mutators...)
	return s, nil

}

func (d Device) hasReportCsv(n SeriesNumber) (bool, error) {
	fn := filepath.Join(d.directory.String(), n.ReportCsv())
	b, e := d.af.Exists(fn)
	if e != nil {
		return false, e
	}
	if !b {
		return false, nil
	}

	return true, nil
}

// getDeviceId will parse a file name and extract the ID of the device, which looks like LBR-0013797.
func getDeviceId(filename string) DeviceId {
	serialNumber, err := parseDeviceIdFromFilename(filename)
	if err != nil {
		return "LBR-0000000"
	}
	return DeviceId(fmt.Sprintf("%s-%s", filename[0:3], serialNumber))
}

// parseDeviceIdFromFilename will pull out the device ID from a filename. An error will be thrown if the
// device ID is not a number.
func parseDeviceIdFromFilename(filename string) (string, error) {
	// TODO [TO20220119] Needs unit tests
	serialNumber := filename[3:10]

	if !util.IsNumericOnly(serialNumber) {
		return "", fmt.Errorf("could not parse a numeric value for the device id from %s", filename)
	}

	return serialNumber, nil
}

// GetSerialNumber is a convenience method that will try to infer the serial number of a Labradar device based on it's
// LBR directory.  Will return an "" if is not possible to do so.
func (d Directory) GetSerialNumber(af *afero.Afero) string {
	f, err := d.getLabradarMarkerFile(af)
	if err != nil {
		return ""
	}

	fn, err := parseDeviceIdFromFilename(f.Name())
	if err != nil {
		return ""
	}

	return getDeviceId(fn).String()
}

// getLabradarMarkerFile will inspect all the files in the LbrDirectory and attempt to find an
// *.LID file that all LBR data folders seem to have.
func (d Directory) getLabradarMarkerFile(af afero.Fs) (afero.File, error) {

	//pattern := filepath.Join(d.String(), "LBR*.LID")
	pattern := "LBR*.LID"
	glob, err := afero.Glob(af, pattern)
	if err != nil {
		return nil, err
	}

	if glob == nil || len(glob) == 0 {
		return nil, fmt.Errorf("Could not find any *.LID files in %s", d.String())
	}
	if len(glob) != 1 {
		logrus.Debugf("Found multiple LID files - grabbing the first one.")
	}

	lidFile := glob[0]
	if !looksLikeTheLabradarMarkerFile(lidFile) {
		return nil, fmt.Errorf("not a valid LID file %s", lidFile)
	}

	f, err := af.Open(lidFile)
	if err != nil {
		return nil, fmt.Errorf(" '%s' does not seem to be an LBR directory", d.String())
	}
	if f == nil {
		return nil, fmt.Errorf("does not seem to be a LBR directory '%s'", d.String())
	}

	return f, nil
}

// looksLikeTheLabradarMarkerFile will inspect a filename and try to infer if the file is the Labradar .LID file.
// The file looks like  LBR0013797201909141617.LID that is a zero-byte file.
func looksLikeTheLabradarMarkerFile(filename string) bool {

	if !strings.HasPrefix(filename, "LBR") {
		return false
	}
	if filepath.Ext(filename) != ".LID" {
		return false
	}

	if len(filename) != 26 {
		return false
	}

	_, err := parseDeviceIdFromFilename(filename)
	if err != nil {
		return false
	}
	return true
}
