package labradar

import (
	"fmt"
	"opgenorth.net/tomutil/stringhelpers"
	"strconv"
)

const seriesNumberLength = 4

// SeriesNumber is a custom type that represents a Labradar series.
type SeriesNumber int

func (t *SeriesNumber) String() string {
	return fmt.Sprintf(stringhelpers.PadLeft(t.Int(), seriesNumberLength))
}
func (t *SeriesNumber) Int() int64 {
	return int64(*t)
}
func (t *SeriesNumber) SeriesFile() string {
	return fmt.Sprintf("series%s.csv", t.String())
}

func (t *SeriesNumber) ShotsFile() string {
	return fmt.Sprintf("series%sShots.csv", t.Int())
}

// TryParseFileNameToSerialNumber will attempt to convert the string into a SeriesNumber.
func TryParseFileNameToSerialNumber(sr string) (SeriesNumber, bool) {
	if len(sr) != 6 {
		return SeriesNumber(0), false
	}
	if sr[0:2] != "SR" {
		return SeriesNumber(0), false
	}

	i, err := strconv.Atoi(sr[2:6])
	if err != nil {
		return 0, false
	}

	return SeriesNumber(i), true
}
