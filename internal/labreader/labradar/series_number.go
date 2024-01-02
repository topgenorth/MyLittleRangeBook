package labradar

import (
	"fmt"
	"labreader/internal/util"
)

// SeriesNumber is a custom type that represents a Labradar series.
type SeriesNumber int

func (t *SeriesNumber) String() string {
	return fmt.Sprintf("#" + util.PadLeft(t.Int(), 4))
}
func (t *SeriesNumber) Int() int64 {
	return int64(*t)
}
func (t *SeriesNumber) SeriesFile() string {
	return fmt.Sprintf("series%s.csv", t.String())
}

func (t *SeriesNumber) ShotsFile() string {
	return fmt.Sprintf("series%shots.csv", t.Int())
}
