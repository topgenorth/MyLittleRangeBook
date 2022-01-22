package device

import (
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/labradar/fs"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
)

// Device holds the details about a specific Labradar device.
type Device struct {
	//DeviceId is a unique ID that is assigned to each Labradar device.
	DeviceId string `json:"deviceId"`
	// Timezone is the timezone that the clock on the Labradar is set to.
	TimeZone string `json:"timezone"`
	// Directory is the name of the folder that holds the Labradar files.
	Directory LbrDirectory `json:"directory"`
	af        *afero.Afero
}

func (d Device) String() string {
	return d.DeviceId
}

// New will initialize a Device structure. The path parameter is the path to the LBR folder
// of the Labradar device.
func New(path string, af *afero.Afero, timezone string) (*Device, error) {

	lbrDirectory, err := tryParseLbrDirectory(path, af)
	if err != nil {
		return nil, err
	}

	f, err := lbrDirectory.GetLabradarMarkerFile(af)
	if err != nil {
		return nil, err
	}

	return &Device{
		DeviceId:  getDeviceId(f.Name()),
		TimeZone:  timezone,
		Directory: lbrDirectory,
		af:        af,
	}, nil
}

// LoadSeries will load the specified series from the Labradar Device.
func (d Device) LoadSeries(seriesNumber int) (*series.LabradarSeries, error) {

	csvValues, err := fs.GetMutatorsToUpdateSeries(seriesNumber, d.Directory.String(), d.af)

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
