package labradar

import (
	"opgenorth.net/labradar/util"
	"time"
)

type LabradarSeries struct {
	Number   int
	Labradar *Labradar
	Firearm  *Firearm
	LoadData *LoadData
	Notes    string
	Tags     []string
}

func Create(seriesNumber int) *LabradarSeries {

	ls := &LabradarSeries{
		Number:   seriesNumber,
		Labradar: initLabradarStruct(seriesNumber),
		Firearm:  nil,
		LoadData: nil,
		Notes:    "",
		Tags:     nil,
	}

	return ls
}

func initLabradarStruct(seriesNumber int) *Labradar {

	loc, _ := time.LoadLocation("UTC")
	now := time.Now().In(loc)

	return &Labradar{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		util.FormatLabradarSeriesNumber(seriesNumber),
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
