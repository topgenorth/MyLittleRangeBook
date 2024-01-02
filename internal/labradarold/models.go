package labradarold

import (
	"fmt"
	"labreader/internal/mathhelpers"
	//"opgenorth.net/mylittlerangebook/pkg/util"
)

// BallisticCoefficient captures the ballistics data about a specific projectile.
type BallisticCoefficient struct {
	DragModel string  `json:"dragModel"`
	Value     float32 `json:"value"`
}

func (t BallisticCoefficient) String() string {
	return fmt.Sprintf("%s 0.3%f", t.DragModel, t.Value)
}

// emptyBC will initialize an empty BallisticCoefficient struct.
func emptyBC() *BallisticCoefficient {
	return &BallisticCoefficient{
		DragModel: "",
		Value:     0,
	}
}

type Projectile struct {
	Name   string                `json:"name"`
	Weight int                   `json:"weight"`
	BC     *BallisticCoefficient `json:"bc"`
}

func (t Projectile) String() string {
	if len(t.Name) > 0 {
		return fmt.Sprintf("%dgr %s", t.Weight, t.Name)
	}
	return fmt.Sprintf("%dgr ?", t.Weight)

}

// emptyProjectile will initialize an empty Projecticle struct.
func emptyProjectile() *Projectile {
	return &Projectile{
		Name:   "",
		Weight: 0,
		BC:     emptyBC(),
	}
}

// PowderCharge holds the data to describe the gun powder in a given LoadData struct
type PowderCharge struct {
	Name   string  `json:"name"`
	Amount float32 `json:"amount"`
}

func (pc PowderCharge) String() string {
	return fmt.Sprintf("%3.1fgr %s", pc.Amount, pc.Name)
}

// LoadData holds a reloading recipe
type LoadData struct {
	Cartridge  string        `json:"cartridge"`
	Projectile *Projectile   `json:"projectile"`
	Powder     *PowderCharge `json:"powder"`
	CBTO       float32       `json:"cbto"`
}

func (l LoadData) String() string {

	var bullet string
	if l.Projectile.Weight > 0 {
		bullet = l.Projectile.String()
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

// emptyLoadData will initialize an empty LoadData struct.
func emptyLoadData() *LoadData {
	return &LoadData{
		Cartridge:  "",
		Projectile: emptyProjectile(),
		Powder: &PowderCharge{
			Name:   "",
			Amount: 0,
		},
		CBTO: 0,
	}
}

// Firearm holds the data to describe a firearm.
type Firearm struct {
	Name      string `json:"name"`
	Cartridge string `json:"cartridge"`
}

func (t Firearm) String() string {
	return fmt.Sprintf("%s (%s)", t.Name, t.Cartridge)
}

// UnitsOfMeasure is used to capture the default units of measure for different pieces of data.
type UnitsOfMeasure struct {
	Velocity    string `json:"velocity"`
	Distance    string `json:"distance"`
	Weight      string `json:"weight"`
	Temperature string `json:"temperature"`
}

func (t UnitsOfMeasure) String() string {
	return fmt.Sprintf("%s/%s/%s/%s", t.Velocity, t.Distance, t.Weight, t.Temperature)
}

// emptyUnitsOfMeasure will initialize an empty UnitsOfMeasure struct.
func emptyUnitsOfMeasure() *UnitsOfMeasure {
	u := &UnitsOfMeasure{
		Velocity: "",
		Distance: "",
		Weight:   "",
	}
	return u
}

// VelocityData holds a list of velocities.
type VelocityData struct {
	Values []int `json:"values"`
}

func (vd VelocityData) CountOfShots() int {
	return len(vd.Values)
}
func (vd VelocityData) String() string {
	return fmt.Sprintf("%d velocities with an average of %d", len(vd.Values), vd.Average())
}

// Append will append the new velocity value to the set.
func (vd *VelocityData) Append(velocity int) {
	vd.Values = append(vd.Values, velocity)
}

// StdDev will return the standard deviation for a set of VelocityData.
func (vd VelocityData) StdDev() float64 {
	return mathhelpers.CalculateStdDevOfInts(vd.Values)
}

// emptyVelocityData will initialize an empty VelocityData struct.
func emptyVelocityData() *VelocityData {
	v := &VelocityData{
		Values: nil,
	}
	return v
}

// ExtremeSpread will return the extreme spread for a set of VelocityData.
func (vd VelocityData) ExtremeSpread() int {
	min, max := mathhelpers.GetMinAndMaxForInts(vd.Values)
	return max - min
}

// Average will return the average for a set of VelocityData.
func (vd VelocityData) Average() int {
	return int(mathhelpers.CalculateAverageOfInts(vd.Values))
}

// Min will return the minimum velocity in a set of VelocityData.
func (vd VelocityData) Min() int {
	_, min := mathhelpers.GetMinAndMaxForInts(vd.Values)
	return min
}

// Max will return the maximum velocity in a set of VelocityData.
func (vd VelocityData) Max() int {
	max, _ := mathhelpers.GetMinAndMaxForInts(vd.Values)
	return max
}
