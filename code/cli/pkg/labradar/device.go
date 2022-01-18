package labradar

import (
	"fmt"
	"github.com/carolynvs/aferox"
	"opgenorth.net/mylittlerangebook/pkg/context"
	"opgenorth.net/mylittlerangebook/pkg/labradar/fs"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"opgenorth.net/mylittlerangebook/pkg/math"
	"os"
	"path/filepath"
	"strings"
)

// Device holds the details about a specific Labradar device.
type Device struct {
	//DeviceId is a unique ID that is assigned to each Labradar device.
	DeviceId string `json:"deviceId"`
	// Timezone is the timezone that the clock on the Labradar is set to.
	TimeZone string `json:"timezone"`
	// Directory is the name of the folder that holds the Labradar files.
	Directory string `json:"directory"`
	af        aferox.Aferox
}

func (d Device) String() string {
	return d.DeviceId
}

func NewDevice(path string, ctx *context.AppContext) (*Device, error) {

	file, err := findTheLabradarMarkerFile(path, ctx.FileSystem)
	if err != nil {
		return nil, fmt.Errorf("does not seem to be a Labradar directory '%s': %w", path, err)
	}
	if file == nil {
		return nil, fmt.Errorf("does not seem to be a Labradar directory '%s'", path)
	}

	return &Device{
		DeviceId:  getDeviceId(file.Name()),
		TimeZone:  ctx.Timezone,
		Directory: path,
		af:        ctx.FileSystem}, nil
}

func (d Device) LoadSeries(seriesNumber int) (*series.LabradarSeries, error) {

	csvValues, err := fs.GetMutatorsToUpdateSeries(seriesNumber, d.Directory, d.af)

	if err != nil {
		e := series.SeriesError{
			Msg:    fmt.Sprintf("could not load the series %d from  device %s (%s): %v", seriesNumber, d.DeviceId, d.Directory, err),
			Number: seriesNumber,
		}
		return nil, e
	}

	mutators := series.MergeMutators(series.LabradarSeriesDefaults(), csvValues)
	s := series.New(mutators...)
	return s, nil

}

//UpdateDeviceForSeries will initialize the Labradar device id and
func UpdateDeviceForSeries(device *Device) series.LabradarSeriesMutatorFunc {
	return func(s *series.LabradarSeries) {
		s.DeviceId = device.DeviceId
	}
}

// findTheLabradarMarkerFile will inspect all the files in a given directory and attempt to find an
// *.LID file that all of the LBR data folders seem to have.
func findTheLabradarMarkerFile(path string, af aferox.Aferox) (os.FileInfo, error) {
	b, err := af.Exists(path)
	if err != nil || !b {
		return nil, fmt.Errorf("could not determine if '%s' is Labradar directory: %w", path, err)
	}

	d, err := af.Open(path)
	if err != nil {
		return nil, fmt.Errorf("could not determine if '%s' is Labradar directory: %w", path, err)
	}
	files, err := d.Readdir(0)
	if err != nil {
		return nil, fmt.Errorf("could not determine if '%s' is Labradar directory: %w", path, err)
	}

	for _, f := range files {
		if f.IsDir() {
			break
		}

		if f.Size() != 0 {
			break
		}

		if looksLikeTheLabradarMarkerFile(f.Name()) {
			return f, nil
		}
	}

	return nil, nil
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
	serialNumber := filename[3:10]

	if !math.IsNumericOnly(serialNumber) {
		return "", fmt.Errorf("could not parse a numeric value for the device id from %s", filename)
	}

	return serialNumber, nil
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
