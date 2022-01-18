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

func (s LabradarSeries) String() string {
	return s.SeriesName()
}
func (s LabradarSeries) SeriesName() string {
	return fmt.Sprintf("%04d", s.Number)
}
func (s LabradarSeries) TotalNumberOfShots() int {
	return len(s.Velocities.Values)
}

// New will take a collection of LabradarSeriesMutatorFunc functions, create a new LabradarSeries, and then
// update it accordingly.
func New(mutators ...LabradarSeriesMutatorFunc) *LabradarSeries {
	s := newSeries()

	for _, mutate := range mutators {
		mutate(s)
	}
	return s
}

// newSeries should create new LabradarSeries that is initialized to primitive default values.
func newSeries() *LabradarSeries {
	s := &LabradarSeries{
		Number:     0,
		Velocities: newVelocityData(),
		Firearm: &Firearm{
			Name:      "",
			Cartridge: "",
		},
		LoadData:       newLoadData(),
		UnitsOfMeasure: newUnitsOfMeasure(),
		Notes:          "",
		Date:           "",
		Time:           "",
	}
	return s
}
