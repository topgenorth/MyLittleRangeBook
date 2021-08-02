package labradar

import (
	"opgenorth.net/labradar/util"
	"path/filepath"
	"time"
)

type Series struct {
	Number   int
	Labradar *Labradar
	Firearm  *Firearm
	LoadData *LoadData
	Notes    string
	Tags     []string
	Data     map[int]*LineOfData
}

func NewSeries() *Series {

	ls := &Series{
		Number:   0,
		Labradar: initLabradarStruct(0),
		Firearm: &Firearm{
			Name:      "",
			Cartridge: "",
		},
		LoadData: &LoadData{
			Cartridge: "",
			Projectile: &Projectile{
				Name:   "",
				Weight: 0,
				BC: &BallisticCoefficient{
					DragModel: "",
					Value:     0,
				},
			},
			Powder: &PowderCharge{
				Name:   "",
				Amount: 0,
			},
		},
		Notes: "",
		Tags:  make([]string, 10), // making a wild guess at how many tags we'll need.
		Data:  make(map[int]*LineOfData),
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
		&UnitsOfMeasure{
			Velocity: "fps",
			Distance: "m",
			Weight:   "gr (grains)",
		},
		&Velocities{
			Average:            0,
			Max:                0,
			Min:                0,
			ExtremeSpread:      0,
			StandardDeviation:  0,
			VelocitiesInSeries: nil,
		},
	}
}

func (ls *Series) JsonFileName(directory string) string {
	filename := filepath.Join(directory, ls.Labradar.SeriesName+".json")
	return filename
}

func (ls *Series) ParseLine(ld *LineOfData) {
	switch ld.LineNumber {
	case 1:
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.DeviceId = ld.GetString()
	case 3:
		ls.Data[ld.LineNumber] = ld
		ls.Number = ld.GetInt()
		ls.Labradar.SeriesName = util.FormatLabradarSeriesNumber(ls.Number)
	case 4:
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.TotalNumberOfShots = ld.GetInt()
	case 6:
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.Units.Velocity = ld.GetString()
	case 7:
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.Units.Distance = ld.GetString()
	case 9:
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.Units.Weight = ld.GetString()
	case 18:
		// For now, we only care about V0 (i.e. the muzzle velocity).
		ls.Data[ld.LineNumber] = ld
		ls.Labradar.Stats.AddVelocity(ld.GetInt())

		// We also pull the date and time from the first shot recorded
		ls.Labradar.Date, ls.Labradar.Time = ld.GetDateAndTime()

	default:
		if ld.LineNumber > 18 {
			ls.Data[ld.LineNumber] = ld
			ls.Labradar.Stats.AddVelocity(ld.GetInt())
		}
	}
}
