package labradar

import (
	"fmt"
	"opgenorth.net/labradar/pkg"
	"opgenorth.net/labradar/pkg/config"
	"strings"
	"time"
)

func NewSeries(seriesNumber int, cfg *config.Config) *Series {

	ls := &Series{
		Number:   seriesNumber,
		Labradar: initLabradarStruct(seriesNumber, cfg.TimeLocation()),
		Velocities: &VelocityData{
			Average:           0,
			Max:               0,
			Min:               0,
			ExtremeSpread:     0,
			StandardDeviation: 0,
			Values:            nil,
		},
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
		Notes:   "",
		Tags:    make([]string, 10), // making a wild guess at how many tags we'll need.
		RawData: make(map[int]*LineOfData),
	}

	return ls
}

func fixupLabradarLine(line string) string {
	parts := strings.Split(strings.TrimSpace(line), pkg.UnicodeNUL)

	switch lengthOfParts := len(parts); lengthOfParts {
	case 2:
		// The string either started with a NUL or ended with a NUL
		if len(parts[0]) == 0 {
			return parts[1]
		}
		if len(parts[1]) == 0 {
			return parts[0]
		}

		return line
	case 3:
		// The string started with NUL and and ended with NUL
		return parts[1]
	default:

		return line
	}

}

func (ls *Series) parseLineOfTextFromLabradarCsv(ld *LineOfData) {
	switch ld.LineNumber {
	case 1:
		ls.RawData[ld.LineNumber] = ld
		ls.Labradar.DeviceId = ld.getStringValue()
	case 3:
		ls.RawData[ld.LineNumber] = ld
	case 6:
		ls.RawData[ld.LineNumber] = ld
		ls.Labradar.Units.Velocity = ld.getStringValue()
	case 7:
		ls.RawData[ld.LineNumber] = ld
		ls.Labradar.Units.Distance = ld.getStringValue()
	case 9:
		ls.RawData[ld.LineNumber] = ld
		ls.Labradar.Units.Weight = ld.getStringValue()
	case 18:
		// For now, we only care about V0 (i.e. the muzzle velocity).
		ls.RawData[ld.LineNumber] = ld
		ls.Velocities.AddVelocity(ld.getIntValue())

		// We also pull the date and time from the first shot recorded
		ls.Labradar.Date, ls.Labradar.Time = ld.getDateAndTime()

	default:
		if ld.LineNumber > 18 {
			ls.RawData[ld.LineNumber] = ld
			ls.Velocities.AddVelocity(ld.getIntValue())
		}
	}
}

func initLabradarStruct(seriesNumber int, timezone *time.Location) *Device {
	now := time.Now().In(timezone)

	return &Device{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		timezone.String(),
		fmt.Sprintf("SR%04d", seriesNumber),
		&UnitsOfMeasure{
			Velocity: "fps",
			Distance: "m",
			Weight:   "gr (grains)",
		},
	}
}
