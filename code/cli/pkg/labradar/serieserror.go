package labradar

import "fmt"

type SeriesError struct {
	Msg    string
	Number int
}

func (s SeriesError) Error() string {
	return fmt.Sprintf("There was a problem trying to process series %d: %s.", s.Number, s.Msg)
}
