package series

import (
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/math"
)

// BallisticCoefficient captures the ballistics data about a specific projectile.
type BallisticCoefficient struct {
	DragModel string  `json:"dragModel"`
	Value     float32 `json:"value"`
}

func (t BallisticCoefficient) String() string {
	return fmt.Sprintf("%s 0.3%f", t.DragModel, t.Value)
}

type Projectile struct {
	Name   string                `json:"name"`
	Weight int                   `json:"weight"`
	BC     *BallisticCoefficient `json:"bc"`
}

func (t Projectile) String() string {
	return fmt.Sprintf("%dgr %s", t.Weight, t.Name)
}

type PowderCharge struct {
	Name   string  `json:"name"`
	Amount float32 `json:"amount"`
}

func (t PowderCharge) String() string {
	return fmt.Sprintf("3.1%fgr %s", t.Amount, t.Name)
}

//LoadData holds a reloading recipe
type LoadData struct {
	Cartridge  string        `json:"cartridge"`
	Projectile *Projectile   `json:"projectile"`
	Powder     *PowderCharge `json:"powder"`
	CBTO       float32       `json:cbto`
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

type Firearm struct {
	Name      string `json:"name"`
	Cartridge string `json:"cartridge"`
}

func (t Firearm) String() string {
	return fmt.Sprintf("%s (%s)", t.Name, t.Cartridge)
}

type UnitsOfMeasure struct {
	Velocity string `json:"velocity"`
	Distance string `json:"distance"`
	Weight   string `json:"weight"`
}

func (t UnitsOfMeasure) String() string {
	return fmt.Sprintf("%s/%s/%s", t.Velocity, t.Distance, t.Weight)
}

type VelocityData struct {
	Average           int     `json:"average"`
	Max               int     `json:"max"`
	Min               int     `json:"min"`
	ExtremeSpread     int     `json:"extremeSpread"`
	StandardDeviation float64 `json:"standardDeviation"`
	Values            []int   `json:"values"`
}

func (stats *VelocityData) AddVelocity(velocity int) {
	stats.Values = append(stats.Values, velocity)
	min, max := math.GetMaxAndMin(stats.Values)

	stats.Average = int(math.CalculateAverage(stats.Values))
	stats.Max = max
	stats.Min = min
	stats.ExtremeSpread = max - min
	stats.StandardDeviation = math.CalculateStandardDeviation(stats.Values)
}
