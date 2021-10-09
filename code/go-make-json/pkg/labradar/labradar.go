package labradar

import (
	"fmt"
	"opgenorth.net/labradar/pkg"
	"strings"
	"time"
)

func NewSeries(seriesNumber int, timeLocation *time.Location) *Series {

	lr := initDevice(seriesNumber, timeLocation)
	vd := &VelocityData{
		Average:           0,
		Max:               0,
		Min:               0,
		ExtremeSpread:     0,
		StandardDeviation: 0,
		Values:            nil,
	}
	f := &Firearm{
		Name:      "",
		Cartridge: "",
	}
	pr := &Projectile{
		Name:   "",
		Weight: 0,
		BC: &BallisticCoefficient{
			DragModel: "",
			Value:     0,
		},
	}
	po := &PowderCharge{
		Name:   "",
		Amount: 0,
	}

	ld := &LoadData{
		Cartridge:  "",
		Projectile: pr,
		Powder:     po,
	}

	ls := &Series{
		Number:     seriesNumber,
		Labradar:   lr,
		Velocities: vd,
		Firearm:    f,
		LoadData:   ld,
		Notes:      "",
		Tags:       make([]string, 10), // making a wild guess at how many tags we'll need.
		RawData:    make(map[int]*LineOfData),
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

func initDevice(seriesNumber int, timezone *time.Location) *Device {
	now := time.Now().In(timezone)

	u := &UnitsOfMeasure{
		Velocity: "fps",
		Distance: "m",
		Weight:   "gr (grains)",
	}
	return &Device{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		timezone.String(),
		fmt.Sprintf("SR%04d", seriesNumber),
		u,
	}
}
