package labradar

import (
	"opgenorth.net/labradar/fs"
	"time"
)

type LabradarSeries struct {
	Labradar *Labradar
	Firearm  *Firearm
	LoadData *LoadData
	Notes    string
	Tags     []string
}

func Create(seriesNumber int) *LabradarSeries {
	series := loadSeries(32)
	return series
}


func loadSeries(seriesNumber int) *LabradarSeries {
	//contents := fs.LoadLabradarSeries(seriesNumber)

	return &LabradarSeries{
		Labradar: initLabradarStruct(seriesNumber),
		Firearm:  &Firearm{},
		LoadData: &LoadData{},
		Notes:    "",
		Tags:     nil,
	}
}

func initLabradarStruct(seriesNumber int) *Labradar {

	loc, _ := time.LoadLocation("UTC")
	now := time.Now().In(loc)

	return &Labradar{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		fs.FormatLabradarSeriesNumber(seriesNumber),
		0,
		&LabradarUnits{
			Velocity: "fps",
			Distance: "m",
			Weight:   "gr (grains)",
		},
		&LabradarVelocity{
			Average:            0,
			Max:                0,
			Min:                0,
			ExtremeSpread:      0,
			StandardDeviation:  0,
			VelocitiesInSeries: nil,
		},
	}
}
