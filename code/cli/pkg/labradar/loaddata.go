package labradar

import (
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
)

type LoadData struct {
	Cartridge  string             `json:"cartridge"`
	Projectile *series.Projectile `json:"projectile"`
	Powder     *PowderCharge      `json:"powder"`
	CBTO       float32            `json:cbto`
}

func (l LoadData) String() string {

	var bullet string
	if l.Projectile.Weight > 0 {
		bullet = fmt.Sprintf("%dgr %s",
			l.Projectile.Weight,
			l.Projectile.Name,
		)
	} else {
		bullet = "Unknown projectile"
	}

	var powder string
	if l.Powder.Amount > 0 {
		powder = fmt.Sprintf("%2.1f gr %s",
			l.Powder.Amount,
			l.Powder.Name,
		)
	} else {
		powder = "Unknown powder"

	}

	var cbto string
	if l.CBTO > 0 {
		cbto = fmt.Sprintf("%2.3f\" CBTO", l.CBTO)
	} else {
		cbto = "Unknown CBTO"
	}

	str := fmt.Sprintf("%s; %s; %s; %s",
		l.Cartridge,
		bullet,
		powder,
		cbto,
	)
	return str
}
