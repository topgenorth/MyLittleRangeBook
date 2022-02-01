package device

import (
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/labradar/fs"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"path/filepath"
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
func (d Device) LoadSeries(n series.Number) (*series.LabradarSeries, error) {

	b, err := d.HasSeries(n)
	if err != nil {

		return nil, err
	}
	if !b {
		e := series.Error{
			Msg:    fmt.Sprintf("%d is not a valid series on the device %s (%s): %v", n, d.DeviceId, d.Directory, err),
			Number: int(n),
		}
		return nil, e
	}

	csvValues, err := fs.GetMutatorsToUpdateSeries(int(n), d.Directory.String(), d.af)

	if err != nil {
		e := series.Error{
			Msg:    fmt.Sprintf("could not load the series %d from  device %s (%s): %v", n, d.DeviceId, d.Directory, err),
			Number: int(n),
		}
		return nil, e
	}

	mutators := series.MergeMutators(series.LabradarSeriesDefaults(), csvValues)
	s := series.New(mutators...)
	return s, nil

}

// HasSeries
func (d Device) HasSeries(number series.Number) (bool, error) {
	seriesDir := filepath.Join(d.Directory.String(), number.SeriesName())

	b, e := d.af.Exists(seriesDir)
	if e != nil {
		return false, e
	}
	if !b {
		return false, nil
	}

	b, e = d.af.IsDir(seriesDir)
	if e != nil {
		return false, e
	}
	if !b {
		return false, nil
	}

	fn := fmt.Sprintf("%s Report.csv", number.SeriesName())
	b, e = d.af.Exists(fn)
	if e != nil {
		return false, e
	}
	if !b {
		return false, nil
	}

	return true, nil
}
