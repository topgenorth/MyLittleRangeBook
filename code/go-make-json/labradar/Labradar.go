package labradar

type Labradar struct {
	DeviceId           string
	Date               string
	Time               string
	SeriesName         string
	TotalNumberOfShots int
	Units              *UnitsOfMeasure
	Stats              *Velocities
}
