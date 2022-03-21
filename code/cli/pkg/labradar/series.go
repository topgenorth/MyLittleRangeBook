// Package series holds all the code for detailing with a Labradar series.
package labradar

// Series is a structure that holds the data from a Labradar series, and some details of the load
// and firearm that was used.
type Series struct {
	Number         SeriesNumber
	DeviceId       string
	Date           string
	Time           string
	Velocities     *VelocityData
	Firearm        *Firearm
	LoadData       *LoadData
	Notes          string
	UnitsOfMeasure *UnitsOfMeasure
}

func (s Series) String() string {
	return s.Number.String()
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

// isSeriesDirectory will return true if the directory name looks like a valid Labradar series directory.
// This is a case sensitive comparison.
//func isSeriesDirectory(dirName string) bool {
//
//
//	if len(dirName) != 6 {
//		return false
//	}
//
//	if dirName[0:2] != "SR" {
//		return false
//	}
//
//	number := dirName[2:]
//	if !util.IsNumericOnly(number) {
//		return false
//	}
//
//	return true
//}

//// FilenameForSeries Given the number of a series and the root directory of the Labradar files, infer the filename of the Labradar
//// CSV file that holds the results of the series.
//func FilenameForSeries(labradarRootDirectory string, seriesNumber int) string {
//	stub := fmt.Sprintf("%04d", seriesNumber)
//	//goland:noinspection SpellCheckingInspection
//	subdir := fmt.Sprintf("SR%s", stub)
//	filename := fmt.Sprintf("SR%s Report.csv", stub)
//	p := path.Join(labradarRootDirectory, subdir, filename)
//	return p
//}
