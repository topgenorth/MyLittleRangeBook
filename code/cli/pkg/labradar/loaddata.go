package labradar

import "fmt"

type LoadData struct {
	Cartridge  string        `json:"cartridge"`
	Projectile *Projectile   `json:"projectile"`
	Powder     *PowderCharge `json:"powder"`
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

	str := fmt.Sprintf("%s; %s; %s",
		l.Cartridge,
		bullet,
		powder,
	)
	return str
}
