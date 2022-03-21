// Package series holds all the code for detailing with a Labradar series.
package labradar

import (
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/util"
	"path"
)

// Series is a structure that holds the data from a Labradar series, and some details of the load
// and firearm that was used.
type Series struct {
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

// String is a simple string representation of the Series
func (s Series) String() string {
	return fmt.Sprintf("%s - %s", s.DeviceId, s.SeriesName())
}

// SeriesName is used to retrieve a Labradar formatted name for the series.
func (s Series) SeriesName() string {
	return Number(s.Number).SeriesName()
}

// TotalNumberOfShots will retrieve the number of shots in the series.
func (s Series) TotalNumberOfShots() int {
	return len(s.Velocities.Values)
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

	s := &Series{
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

	s.Update(mutators...)
	return s
}

// Number represents the series number of a device.
type Number int

// String will return a SeriesName formatted as a string.
func (t Number) String() string {
	return fmt.Sprintf("SR%04d", t)
}

// SeriesName is just an alias for Number.String()
func (t Number) SeriesName() string {
	return t.String()
}

// isSeriesDirectory will return true if the directory name looks like a valid Labradar series directory.
// This is a case sensitive comparison.
func isSeriesDirectory(dirName string) bool {
	if len(dirName) != 6 {
		return false
	}

	if dirName[0:2] != "SR" {
		return false
	}

	number := dirName[2:]
	if !util.IsNumericOnly(number) {
		return false
	}

	return true
}

// FilenameForSeries Given the number of a series and the root directory of the Labradar files, infer the filename of the Labradar
// CSV file that holds the results of the series.
func FilenameForSeries(labradarRootDirectory string, seriesNumber int) string {
	stub := fmt.Sprintf("%04d", seriesNumber)
	//goland:noinspection SpellCheckingInspection
	subdir := fmt.Sprintf("SR%s", stub)
	filename := fmt.Sprintf("SR%s Report.csv", stub)
	p := path.Join(labradarRootDirectory, subdir, filename)
	return p
}
