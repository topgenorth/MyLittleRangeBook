package labradarold

import (
	"fmt"
	"labreader/internal/util"
	"labreader/labradar"
	"path/filepath"
	"strings"
)

// Device represents the characteristics of a Labradar device (either by looking at the SD card or by some kind of Bluetooth).
type Device interface {
	DeviceId() DeviceId
	TimeZone() string
	Directory() Directory
	String() string
	LoadSeries(n labradar.SeriesNumber) (*Series, error)
	ListSeries() []labradar.SeriesNumber
}

type DeviceId string

// DeviceDirectory holds the details about a specific Labradar device.
type DeviceDirectory struct {
	//DeviceId is a unique ID that is assigned to each Labradar device.
	deviceId DeviceId
	// Timezone is the timezone that the clock on the Labradar is set to.
	timeZone string
	// Directory is the name of the folder that holds the Labradar files.
	directory Directory
	//af        *afero.Afero
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

func (d *DeviceDirectory) DeviceId() DeviceId {
	return d.deviceId
}

func (d *DeviceDirectory) TimeZone() string {
	return d.timeZone
}

func (d *DeviceDirectory) Directory() Directory {
	return d.directory
}

func (d DeviceDirectory) String() string {
	return d.deviceId.String()
}

// LoadSeries will retrieve the specified series from the Labradar DeviceDirectory.
//func (d *DeviceDirectory) LoadSeries(n SeriesNumber) (*Series, error) {
//
//	if !n.ExistsOn(d) {
//		return nil, fmt.Errorf("%s does not exist on the device %s (%s)", n.String(), d.DeviceId().String(), d.FileDirectory())
//	}
//
//	filename := n.pathToReportCsvOn(d)
//
//	file := *loadCsv(filename, d.af)
//	csvValues := []SeriesMutatorFn{
//		withDeviceIdFromCsv(file),
//		withSeriesNumberFromCsv(file),
//		withUnitsOfMeasureFromCsv(file),
//		addMuzzleVelocitiesFromCsv(file),
//		withSeriesDateFromCsv(file),
//	}
//
//	mutators := combineMutators(LabradarSeriesDefaults(), csvValues)
//	s := NewSeries(mutators...)
//	return s, nil
//
//}

//func (d DeviceDirectory) hasReportCsv(n SeriesNumber) (bool, error) {
//	fn := filepath.Join(d.directory.String(), n.ReportCsv())
//	b, e := d.af.Exists(fn)
//	if e != nil {
//		return false, e
//	}
//	if !b {
//		return false, nil
//	}
//
//	return true, nil
//}

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

//func (t *DeviceDirectory) ListSeries() []SeriesNumber {
//
//	return t.directory.SeriesNumbers(t.af)
//}

func (t DeviceId) String() string {
	return string(t)
}

func (t DeviceId) SerialNumber() string {
	return t.String()[4:10]
}
