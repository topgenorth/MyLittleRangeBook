package labradar

type Labradar struct {
	DeviceId           string
	Date               string
	Time               string
	SeriesName         string
	TotalNumberOfShots int
	Units              *LabradarUnits
	Stats              *LabradarVelocity
}
