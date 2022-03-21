package labradar

import (
	"strconv"
	"strings"
)

type ReadmeLine struct {
	LineNumber   int
	SeriesNumber int
	Firearm      *Firearm
	Load         *LoadData
	Text         string
	Err          error
}

func getReadmeLine(text string) *ReadmeLine {

	t := strings.TrimSpace(text)
	var err error = nil
	parts := strings.Split(t, "|")[1:5]

	seriesNumber, err := strconv.Atoi(strings.TrimSpace(parts[0]))
	ld := getLoadData(parts[1])
	f := &Firearm{strings.TrimSpace(parts[2]), ld.Cartridge}

	r := &ReadmeLine{
		LineNumber:   0,
		SeriesNumber: seriesNumber,
		Firearm:      f,
		Load:         ld,
		Text:         t,
		Err:          err,
	}

	return r
}

func getLoadData(ammoPart string) *LoadData {
	ammoParts := strings.Split(ammoPart, ";")

	ld := &LoadData{
		Cartridge:  strings.TrimSpace(ammoParts[0]),
		Projectile: getProjectileFrom(ammoParts[1]),
		Powder:     getPowderChargeFrom(ammoParts[2]),
	}

	return ld

}

func getProjectileFrom(s string) *Projectile {
	p := &Projectile{
		Name:   strings.TrimSpace(s),
		Weight: 0,
		BC:     nil,
	}
	return p
}

func getPowderChargeFrom(s string) *PowderCharge {
	pc := &PowderCharge{
		Name:   strings.TrimSpace(s),
		Amount: 0,
	}

	return pc
}
