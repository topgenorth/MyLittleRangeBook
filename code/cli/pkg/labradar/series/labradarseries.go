// Package series holds all the code for detailing with a Labradar series.
package series

import (
	"fmt"
)

type SeriesError struct {
	Msg    string
	Number int
}

func (s SeriesError) Error() string {
	return fmt.Sprintf("There was a problem trying to process series %d: %s.", s.Number, s.Msg)
}

// LabradarSeries is a structure that holds the data from a Labradar series, and some details of the load
// and firearm that was used.
type LabradarSeries struct {
	Number         int             `json:"number"`
	DeviceId       string          `json:"deviceId"`
	Date           string          `json:"date"`
	Time           string          `json:"time"`
	Velocities     *VelocityData   `json:"velocities"`
	Firearm        *Firearm        `json:"firearm"`
	LoadData       *LoadData       `json:"loadData"`
	Notes          string          `json:"notes"`
	UnitsOfMeasure *UnitsOfMeasure `json:"unitsOfMeasure"`
}

// String is a simple string representation of the LabradarSeries
func (s LabradarSeries) String() string {
	return fmt.Sprintf("%s - %s", s.DeviceId, s.SeriesName())
}

// SeriesName is used to retrieve a Labradar formated name for the series.
func (s LabradarSeries) SeriesName() string {
	return fmt.Sprintf("%04d", s.Number)
}

// TotalNumberOfShots will retrieve the number of shots in the series.
func (s LabradarSeries) TotalNumberOfShots() int {
	return len(s.Velocities.Values)
}

// Update will use the provided mutators to update values in the LabradarSeries
func (s *LabradarSeries) Update(mutators ...LabradarSeriesMutatorFunc) {
	for _, mutate := range mutators {
		mutate(s)
	}

}

// New will take a collection of LabradarSeriesMutatorFunc functions, create a new LabradarSeries, and then
// update it accordingly.
func New(mutators ...LabradarSeriesMutatorFunc) *LabradarSeries {
	s := newSeries()
	s.Update(mutators...)
	return s
}

// newSeries should create new LabradarSeries that is initialized to primitive default values.
func newSeries() *LabradarSeries {
	s := &LabradarSeries{
		Number:     0,
		Velocities: emptyVelocityData(),
		Firearm: &Firearm{
			Name:      "",
			Cartridge: "",
		},
		LoadData:       emptyLoadData(),
		UnitsOfMeasure: emptyUnitsOfMeasure(),
		Notes:          "",
		Date:           "",
		Time:           "",
	}

	return s
}
