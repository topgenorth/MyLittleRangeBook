package series

import (
	"fmt"
	"github.com/aidarkhanov/nanoid"
	"labreader/internal/ddd"
	"time"
)

type (
	MutatorFunction = func(s *Series)

	// Series holds the data on a series of shots from the Labradar.
	//
	// For now, we assume tha the units are feet-per-second and grains.
	Series struct {
		ddd.AggregateBase
		Number      int
		DeviceId    string
		DateAndTime time.Time
		Velocities  *velocityData
		Notes       string
		dateCreated time.Time
	}
)

func New() (*Series, error) {
	// TODO [TO20231216] validation.
	s := &Series{
		AggregateBase: ddd.AggregateBase{ID: nanoid.New()},
		dateCreated:   time.Now().UTC(),
		Number:        -1,
		DeviceId:      "",
		DateAndTime:   time.Now().UTC(),
		Velocities:    &velocityData{make([]int, 0)},
		Notes:         "",
	}

	s.AddEvent(&SeriesCreated{Series: s})
	return s, nil
}
func New2(mutators ...MutatorFunction) (*Series, error) {
	s, _ := New()
	for _, mutate := range mutators {
		mutate(s)
	}

	return s, nil
}

func (s *Series) String() string {
	return fmt.Sprintf("%s-%d", s.ID, s.Number)
}

// CountOfShots will retrieve the Number of shots in the series.
func (s *Series) CountOfShots() int {
	return s.Velocities.CountOfShots()
}

func (s *Series) AppendVelocity(number int, velocity int) {
	s.Velocities.Values = append(s.Velocities.Values, velocity)
	s.AddEvent(&VelocityAdded{
		Number:   number,
		Velocity: velocity,
	})
}
