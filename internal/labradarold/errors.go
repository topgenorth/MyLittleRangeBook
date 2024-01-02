package labradarold

import (
	"fmt"
	"labreader/labradar"
)

type Error struct {
	Msg    string
	Number int64
}

func (s Error) Error() string {
	return fmt.Sprintf("There was a problem trying to process series %d: %s.", s.Number, s.Msg)
}

type SeriesNotFoundError struct {
	// The series.Number
	Number labradar.SeriesNumber
}

func (e SeriesNotFoundError) Error() string {
	return fmt.Sprintf("%s could not be found.", e.Number.String())
}
