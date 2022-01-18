package series

// LabradarSeriesMutatorFunc describes a function that can be used to manipulate the fields of a LabradarSeries
type LabradarSeriesMutatorFunc = func(s *LabradarSeries)

// New will take a collection of SeriesBuildFunc objects, and use them to create a new LabradarSeries object.
func New(builders ...LabradarSeriesMutatorFunc) *LabradarSeries {
	s := &LabradarSeries{
		Number:     0,
		Velocities: newVelocityData(),
		Firearm: &Firearm{
			Name:      "",
			Cartridge: "",
		},
		LoadData:       newLoadData(),
		UnitsOfMeasure: newUnitsOfMeasure(),
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

// WithSeriesNumber will initialize the number assigned by a specific Labradar device
func WithSeriesNumber(n int) LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.Number = n
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

// UsingGrainsForWeight sets the units of measure to grains.
func UsingGrainsForWeight() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.UnitsOfMeasure.Weight = "gr"
	}
}

// UsingFeetPerSecondForMuzzleVelocity will set the default velocity units to FPS
func UsingFeetPerSecondForMuzzleVelocity() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.UnitsOfMeasure.Velocity = "fps"
	}
}

// UsingMetresPerSecondForMuzzleVelocity will set the default velocity units to m/s
func UsingMetresPerSecondForMuzzleVelocity() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.UnitsOfMeasure.Velocity = "m/s"
	}
}

// UsingYardsForDistance will set the default distance units to yards.
func UsingYardsForDistance() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.UnitsOfMeasure.Distance = "y"
	}
}

// UsingMetresForDistance will set the default distance units to metres.
func UsingMetresForDistance() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.UnitsOfMeasure.Distance = "m"
	}
}

// UsingFeetForDistance will set the default distance units to feet.
func UsingFeetForDistance() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.UnitsOfMeasure.Distance = "ft"
	}
}

func newLoadData() *LoadData {
	return &LoadData{
		Cartridge:  "",
		Projectile: newProjectile(),
		Powder: &PowderCharge{
			Name:   "",
			Amount: 0,
		},
		CBTO: 0,
	}
}
func newUnitsOfMeasure() *UnitsOfMeasure {
	u := &UnitsOfMeasure{
		Velocity: "",
		Distance: "",
		Weight:   "",
	}
	return u
}
func newVelocityData() *VelocityData {
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
func newProjectile() *Projectile {
	return &Projectile{
		Name:   "",
		Weight: 0,
		BC:     newBC(),
	}
}
func newBC() *BallisticCoefficient {
	return &BallisticCoefficient{
		DragModel: "",
		Value:     0,
	}
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
