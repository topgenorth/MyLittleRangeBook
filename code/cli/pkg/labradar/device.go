package labradar

import (
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/util"
	"path/filepath"
	"strings"
)

// NewDevice will initialize a Device structure. The path parameter is the path to the LBR folder
// of the Labradar device.
func NewDevice(dir Directory, fs afero.Fs) (*Device, error) {

	return &Device{
		deviceId:  dir.DeviceId(),
		timeZone:  "America/Edmonton",
		directory: dir,
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

// parseDeviceIdFromFilename will pull out the device ID from a filename. An error will be thrown if the
// device ID is not a number.
func parseDeviceIdFromFilename(filename string) (string, error) {
	// TODO [TO20220119] Needs unit tests
	base := filepath.Base(filename)
	deviceId := base[3:10]

	if !util.IsNumericOnly(deviceId) {
		return "", fmt.Errorf("could not parse a numeric value for the device id from %s", filename)
	}

	return deviceId, nil
}

func looksLikeTheLabradarMarkerFile(filename string) bool {

	base := filepath.Base(filename)
	if !strings.HasPrefix(base, "LBR") {
		return false
	}

	if filepath.Ext(filename) != ".LID" {
		return false
	}

	if len(base) != 26 {
		return false
	}

	_, err := parseDeviceIdFromFilename(filename)
	if err != nil {
		return false
	}
	return true
}
