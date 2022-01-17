package series

import (
	"opgenorth.net/mylittlerangebook/pkg"
	"time"
)

// LabradarSeriesMutatorFunc describes a function that can be used to manipulate the fields of a LabradarSeries
type LabradarSeriesMutatorFunc = func(s *LabradarSeries)

// New will take a collection of SeriesBuildFunc objects, and use them to create a new LabradarSeries object.
func New(builders ...LabradarSeriesMutatorFunc) *LabradarSeries {
	s := &LabradarSeries{
		Number:     0,
		Labradar:   defaultDevice(),
		Velocities: defaultVelocityData(),
		Firearm: &Firearm{
			Name:      "",
			Cartridge: "",
		},
		LoadData:       defaultLoadData(),
		unitsOfMeasure: defaultUnitsOfMeasure(),
		Notes:          "",
	}

	defaults := []LabradarSeriesMutatorFunc{
		UsingGrainsForWeight(),
		UsingMetresForDistance(),
		UsingFeetPerSecondForMuzzleVelocity(),
	}

	for _, builder := range mergeDefaultsAndCustomMutators(defaults, builders) {
		builder(s)
	}
	return s
}

func mergeDefaultsAndCustomMutators(defaults []LabradarSeriesMutatorFunc, builders []LabradarSeriesMutatorFunc) []LabradarSeriesMutatorFunc {

	mutators := make([]LabradarSeriesMutatorFunc, len(defaults)+len(builders))
	index := 0

	for _, m := range defaults {
		mutators[index] = m
		index = index + 1
	}

	for _, b := range builders {
		mutators[index] = b
		index = index + 1
	}

	return mutators
}

// WithSeriesNumber will initialize the number assigned by a specific Labradar device
func WithSeriesNumber(n int) LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.Number = n
	}
}

// WithDevice will initialize the Labrader device id and
func WithDevice(deviceId string, tz *time.Location) LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		now := time.Now().In(tz)
		d := &LabradarDevice{
			DeviceId: deviceId,
			Date:     now.Format("YYYY-MM-DD"),
			Time:     now.Format("15:04"),
			TimeZone: tz.String(),
		}
		s.Labradar = d
	}
}

// WithFirearm will set the cartridge and name of the firearm.
func WithFirearm(name string, cartridge string) LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.Firearm = &Firearm{Name: name, Cartridge: cartridge}
		s.LoadData.Cartridge = cartridge
	}
}

// WithPowder will set the name of the gunpoweder used and weight of the powder charge
func WithPowder(name string, weight float32) LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.LoadData.Powder.Name = name
		s.LoadData.Powder.Amount = weight
	}
}
func UsingGrainsForWeight() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.unitsOfMeasure.Weight = "gr"
	}
}
func UsingFeetPerSecondForMuzzleVelocity() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.unitsOfMeasure.Velocity = "fps"
	}
}
func UsingMetresPerSecondForMuzzleVelocity() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.unitsOfMeasure.Velocity = "m/s"
	}
}

func UsingYardsForDistance() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.unitsOfMeasure.Distance = "y"
	}
}
func UsingMetresForDistance() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.unitsOfMeasure.Distance = "m"
	}
}
func UsingFeetForDistance() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.unitsOfMeasure.Distance = "ft"
	}
}

func defaultDevice() *LabradarDevice {
	tz, _ := time.LoadLocation(pkg.DefaultTimeZone)
	now := time.Now().In(tz)
	return &LabradarDevice{
		DeviceId: "",
		Date:     now.Format("YYYY-MM-DD"),
		Time:     now.Format("15:04"),
		TimeZone: pkg.DefaultTimeZone,
	}
}
func defaultLoadData() *LoadData {
	return &LoadData{
		Cartridge:  "",
		Projectile: defaultProjectile(),
		Powder: &PowderCharge{
			Name:   "",
			Amount: 0,
		},
		CBTO: 0,
	}
}
func defaultUnitsOfMeasure() *UnitsOfMeasure {
	u := &UnitsOfMeasure{
		Velocity: "",
		Distance: "",
		Weight:   "",
	}
	return u
}
func defaultVelocityData() *VelocityData {
	v := &VelocityData{
		Average:           0,
		Max:               0,
		Min:               0,
		ExtremeSpread:     0,
		StandardDeviation: 0,
		Values:            nil,
	}
	return v
}
func defaultProjectile() *Projectile {
	return &Projectile{
		Name:   "",
		Weight: 0,
		BC:     defaultBC(),
	}
}
func defaultBC() *BallisticCoefficient {
	return &BallisticCoefficient{
		DragModel: "",
		Value:     0,
	}
}
