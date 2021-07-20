package domain

type PowderCharge struct {
	Name   string
	Amount float32
}

type LabradarUnits struct {
	Velocity string
	Distance string
	Weight   string
}

type LabradarSeriesStats struct {
	Average           int
	Max               int
	Min               int
	ExtremeSpread     int
	StandardDeviation float32
}

type Labradar struct {
	DeviceId           string
	Date               string
	Time               string
	SeriesName         string
	TotalNumberOfShots int
	Units              LabradarUnits
	Stats              LabradarSeriesStats
	VelocitiesInSeries []int
}

type BallisticCoefficient struct {
	DragModel string
	Value     float32
}
type LoadData struct {
	Cartridge  string
	Projectile Projectile
	Powder     PowderCharge
}

type Projectile struct {
	Name   string
	Weight int
	BC     BallisticCoefficient
}

type Firearm struct {
	Name      string
	Cartridge string
}
type LabradarSeries struct {
	Labradar Labradar
	Firearm  Firearm
	LoadData LoadData
	Notes    string
	Tags     []string
}
