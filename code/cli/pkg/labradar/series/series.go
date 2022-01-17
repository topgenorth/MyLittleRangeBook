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

func New() {

}

type LabradarSeries struct {
	Number     int             `json:"number"`
	Labradar   *LabradarDevice `json:"labradar"`
	Velocities *VelocityData   `json:"velocities"`
	Firearm    *Firearm        `json:"firearm"`
	LoadData   *LoadData       `json:"loadData"`
	Notes      string          `json:"notes"`
}
