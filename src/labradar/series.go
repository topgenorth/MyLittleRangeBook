package labradar

import (
	"time"
)

// Series is a structure that holds the data from a Labradar series, and some details of the load
// and firearm that was used.
type Series struct {
	number   SeriesNumber
	deviceId DeviceId
	// dateTime is the date and time of the series, in UTC.
	dateTime       time.Time
	velocities     *velocityData
	notes          string
	unitsOfMeasure *unitsOfMeasure
	// dateCreated is the time of creation (in UTC)
	dateCreated time.Time
}

// SeriesMutatorFn describes a function that can be used to manipulate the values of a Series
type SeriesMutatorFn = func(s *Series)

func (s Series) String() string {
	return s.number.String()
}

func (s Series) DeviceId() DeviceId {
	return s.deviceId
}

// CountOfShots will retrieve the number of shots in the series.
func (s Series) CountOfShots() int {
	return s.velocities.CountOfShots()
}

func (s Series) AverageVelocity() int {
	return s.velocities.Average()
}

// Update will use the provided mutators to update values in the Series
func (s *Series) Update(mutators ...SeriesMutatorFn) {
	for _, mutate := range mutators {
		mutate(s)
	}
}

// NewSeries will take a collection of SeriesMutatorFn functions, create a new Series, and then
// update it accordingly.
func NewSeries(mutators ...SeriesMutatorFn) *Series {

	s := &Series{}
	s.dateCreated = time.Now().UTC()

	s.Update(mutators...)
	return s
}
