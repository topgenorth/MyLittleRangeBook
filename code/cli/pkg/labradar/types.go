package labradar

import (
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/util"
	"strconv"
)

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

func (t *SeriesNumber) ExistsOn(d *Device) (bool, error) {
	return false, nil
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

// Device holds the details about a specific Labradar device.
type Device struct {
	//DeviceId is a unique ID that is assigned to each Labradar device.
	deviceId string
	// Timezone is the timezone that the clock on the Labradar is set to.
	timeZone string
	// Directory is the name of the folder that holds the Labradar files.
	directory Directory
	af        *afero.Afero
}

type DeviceId string

func (t DeviceId) String() string {
	return string(t)
}

func (t DeviceId) SerialNumber() string {
	return t.String()[4:10]
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

const LBRDirectory = "LBR/"
