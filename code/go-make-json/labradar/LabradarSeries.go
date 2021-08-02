package labradar

import (
	"fmt"
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

func (ls *LabradarSeries) ParseLine(ld *LineOfData) {
	switch ld.LineNumber {
	case 1:
		ls.Labradar.DeviceId = ld.GetString()
	case 3:
		ls.Number = ld.GetInt()
		ls.Labradar.SeriesName = util.FormatLabradarSeriesNumber(ls.Number)
	case 4:
		ls.Labradar.TotalNumberOfShots = ld.GetInt()
	case 6:
		ls.Labradar.Units.Velocity = ld.GetString()
	case 7:
		ls.Labradar.Units.Distance = ld.GetString()
	case 9:
		ls.Labradar.Units.Weight = ld.GetString()
	case 18:
		ls.Labradar.Stats.addVelocity(ld.GetInt())
		ls.Labradar.Date, ls.Labradar.Time = ld.GetDateAndTime()

	default:
		if ld.LineNumber > 18 {
			ls.Labradar.Stats.addVelocity(ld.GetInt())
		} else {
			fmt.Printf("%d: %s", ld.LineNumber, ld.CleanValue)
			fmt.Println()
		}
	}
}
