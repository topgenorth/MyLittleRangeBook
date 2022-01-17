package labradar

import (
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"strconv"
	"strings"
)

type ReadmeLine struct {
	LineNumber   int
	SeriesNumber int
	Firearm      *series.Firearm
	Load         *series.LoadData
	Text         string
	Err          error
}

func getReadmeLine(text string) *ReadmeLine {

	t := strings.TrimSpace(text)
	var err error = nil
	parts := strings.Split(t, "|")[1:5]

	seriesNumber, err := strconv.Atoi(strings.TrimSpace(parts[0]))
	ld := getLoadData(parts[1])
	f := &series.Firearm{strings.TrimSpace(parts[2]), ld.Cartridge}

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

func getLoadData(ammoPart string) *series.LoadData {
	ammoParts := strings.Split(ammoPart, ";")

	ld := &series.LoadData{
		Cartridge:  strings.TrimSpace(ammoParts[0]),
		Projectile: getProjectileFrom(ammoParts[1]),
		Powder:     getPowderChargeFrom(ammoParts[2]),
	}

	return ld

}

func getProjectileFrom(s string) *series.Projectile {
	p := &series.Projectile{
		Name:   strings.TrimSpace(s),
		Weight: 0,
		BC:     nil,
	}
	return p
}

func getPowderChargeFrom(s string) *series.PowderCharge {
	pc := &series.PowderCharge{
		Name:   strings.TrimSpace(s),
		Amount: 0,
	}

	return pc
}
