package series

import "time"

// LabradarSeriesMutatorFunc describes a function that can be used to manipulate the values of a LabradarSeries
type LabradarSeriesMutatorFunc = func(s *LabradarSeries)

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

// WithPowder will set the name of the gunpowder used and weight of the powder charge
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

func UsingCurrentDateAndTime() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		now := time.Now()
		s.Date = now.Format("2006-01-02")
		s.Time = now.Format("15:04")
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
