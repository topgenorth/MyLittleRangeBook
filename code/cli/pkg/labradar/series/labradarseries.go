// Package series holds all the code for detailing with a Labradar series.
package series

import (
	"fmt"
	//"opgenorth.net/mylittlerangebook/pkg/labradar"
	//"opgenorth.net/mylittlerangebook/pkg/labradar"
)

type SeriesError struct {
	Msg    string
	Number int
}

func (s SeriesError) Error() string {
	return fmt.Sprintf("There was a problem trying to process series %d: %s.", s.Number, s.Msg)
}

type LabradarSeries struct {
	Number   int    `json:"number"`
	DeviceId string `json:deviceId`
	//Labradar       *labradar.Device `json:"labradar"`
	Velocities     *VelocityData   `json:"velocities"`
	Firearm        *Firearm        `json:"firearm"`
	LoadData       *LoadData       `json:"loadData"`
	Notes          string          `json:"notes"`
	UnitsOfMeasure *UnitsOfMeasure `json:"unitsOfMeasure"`
}

func (s LabradarSeries) String() string {
	return s.SeriesName()
}
func (s LabradarSeries) SeriesName() string {
	return fmt.Sprintf("%04d", s.Number)
}
func (s LabradarSeries) TotalNumberOfShots() int {
	return len(s.Velocities.Values)
}

/*func LoadSeries(device *labradar.Device, seriesNumber int) (*LabradarSeries, error) {
	//filename := fs.FilenameForSeries(device.Directory, seriesNumber)
	//builders, err := fs.FromCsvFile(filename, device.af)
	//
	//if err != nil {
	//	e := SeriesError{
	//		Msg:    fmt.Sprintf("could not load the series %d from  device %s (%s): %w", seriesNumber, device.DeviceId, device.Directory, err),
	//		Number: seriesNumber,
	//	}
	//	return nil, e
	//}
	//
	//s := New(builders...)
	return nil, nil
}*/
