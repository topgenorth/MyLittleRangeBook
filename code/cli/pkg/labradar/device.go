package labradar

import (
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/util"
	"os"
	"path/filepath"
)

// NewDevice will initialize a Device structure. The path parameter is the path to the LBR folder
// of the Labradar device.
func NewDevice(path string, af *afero.Afero, timezone string) (*Device, error) {

	lbrDirectory, err := TryParseDirectory(path, af)
	if err != nil {
		return nil, err
	}

	f, err := lbrDirectory.getLabradarMarkerFile(af)
	if err != nil {
		return nil, err
	}

	return &Device{
		deviceId:  getDeviceId(f.Name()),
		timeZone:  timezone,
		directory: lbrDirectory,
		af:        af,
	}, nil
}

func init() {

}

func (d *Device) DeviceId() string {
	return d.deviceId
}

func (d *Device) TimeZone() string {
	return d.timeZone
}

func (d *Device) Directory() Directory {
	return d.directory
}

func (d Device) String() string {
	return d.deviceId
}

// SeriesNumber will retrieve the specified series from the Labradar Device.
func (d Device) SeriesNumber(n Number) (*Series, error) {

	directory := d.Directory()
	csvValues, err := updateSeriesFromCsvFile(int(n), directory.String(), d.af)

	if err != nil {
		e := Error{
			Msg:    fmt.Sprintf("could not load the series %d from  device %s (%s): %v", n, d.DeviceId(), d.Directory(), err),
			Number: int(n),
		}
		return nil, e
	}

	mutators := MergeMutators(LabradarSeriesDefaults(), csvValues)
	s := NewSeries(mutators...)
	return s, nil

}

func (d Device) hasReportCsv(dir string, number Number) (bool, error) {
	fn := filepath.Join(dir, fmt.Sprintf("%s Report.csv", number.SeriesName()))
	b, e := d.af.Exists(fn)
	if e != nil {
		return false, e
	}
	if !b {
		return false, nil
	}

	return true, nil
}

// getDeviceId will parse a file name and extract the name of the device, which looks like LBR-0013797.
func getDeviceId(filename string) string {
	serialNumber, err := parseDeviceIdFromFilename(filename)
	if err != nil {
		return "LBR-0000000"
	}
	return fmt.Sprintf("%s-%s", filename[0:3], serialNumber)
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

	return getDeviceId(fn)
}

// getLabradarMarkerFile will inspect all the files in the LbrDirectory and attempt to find an
// *.LID file that all LBR data folders seem to have.
func (d Directory) getLabradarMarkerFile(af *afero.Afero) (os.FileInfo, error) {
	f, err := findTheLabradarMarkerFile(d.String(), af)
	if err != nil {
		return nil, fmt.Errorf(" '%s' does not seem to be an LBR directory", d.String())
	}
	if f == nil {
		return nil, fmt.Errorf("does not seem to be a LBR directory '%s'", d.String())
	}

	return f, nil
}
