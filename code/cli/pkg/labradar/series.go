package labradar

// Series is a structure that holds the data from a Labradar series, and some details of the load
// and firearm that was used.
type Series struct {
	Number         SeriesNumber
	deviceId       DeviceId
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
func (s *Series) DeviceId() DeviceId {
	return s.deviceId
}

// TotalNumberOfShots will retrieve the number of shots in the series.
func (s *Series) TotalNumberOfShots() int {
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
