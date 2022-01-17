// Package series holds all the code for detailing with a Labradar series.
package series

import "fmt"

type SeriesError struct {
	Msg    string
	Number int
}

func (s SeriesError) Error() string {
	return fmt.Sprintf("There was a problem trying to process series %d: %s.", s.Number, s.Msg)
}

type LabradarSeries struct {
	Number         int             `json:"number"`
	Labradar       *LabradarDevice `json:"labradar"`
	Velocities     *VelocityData   `json:"velocities"`
	Firearm        *Firearm        `json:"firearm"`
	LoadData       *LoadData       `json:"loadData"`
	Notes          string          `json:"notes"`
	unitsOfMeasure *UnitsOfMeasure
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
