package series

import (
	"time"
)

// LabradarSeriesMutatorFunc describes a function that can be used to manipulate the values of a LabradarSeries
type LabradarSeriesMutatorFunc = func(s *LabradarSeries)

// WithSeriesNumber will initialize the number assigned by a specific Labradar device
func WithSeriesNumber(n int) LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.Number = n
	}
}

// WithFirearm will set the cartridge and name of the firearm.  This does not update the cartridge on the LoadData
func WithFirearm(name string) LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.Firearm.Name = name
	}
}

// WithCartridge will set the cartridge of the LoadData. This does update the cartridge on the Firearm.
func WithCartridge(cartridge string) LabradarSeriesMutatorFunc {
	// TODO [TO20220123] What should we do if the cartridge cartridge doesn't match the cartridge on the firearm?
	return func(s *LabradarSeries) {
		s.Firearm.Cartridge = cartridge
		s.LoadData.Cartridge = cartridge
	}
}

// WithNotes will update the notes field.
func WithNotes(notes string) LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.Notes = notes
	}
}

// WithPowder will set the name of the gunpowder used and weight on the PowderCharge.
func WithPowder(name string, weight float32) LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.LoadData.Powder.Name = name
		s.LoadData.Powder.Amount = weight
	}
}

// WithProjecticle will set the name and weight of the projectile on the Projectile.
func WithProjecticle(name string, weight int) LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.LoadData.Projectile.Name = name
		s.LoadData.Projectile.Weight = weight
	}
}

// UsingGrainsForWeight sets the units of measure to grains.
func UsingGrainsForWeight() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.UnitsOfMeasure.Weight = "gr"
	}
}

// UsingCurrentDateAndTime will use the time from this process as the default.
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

// LabradarSeriesDefaults returns the mutators that will set default values on a LabradarSeries.
func LabradarSeriesDefaults() []LabradarSeriesMutatorFunc {
	defaults := []LabradarSeriesMutatorFunc{
		UsingGrainsForWeight(),
		UsingMetresForDistance(),
		UsingFeetPerSecondForMuzzleVelocity(),
		UsingCurrentDateAndTime(),
		UsingCelsiusForTemperature(),
	}

	return defaults
}

// UsingCelsiusForTemperature will return a mutator to set the temperature Units of Measure to Centigrade.
func UsingCelsiusForTemperature() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.UnitsOfMeasure.Temperature = "Celsius"
	}
}

// UsingFarenheitForTemperature will return a mutator to set the temperature Units of Measure to Fahrenheit.
func UsingFarenheitForTemperature() LabradarSeriesMutatorFunc {
	return func(s *LabradarSeries) {
		s.UnitsOfMeasure.Temperature = "Fahrenheit"
	}
}

// MergeMutators will combine two separate arrays of LabradarSeriesMutatorFunc into one.  The items in the first
// array will appear first.
func MergeMutators(first []LabradarSeriesMutatorFunc, second []LabradarSeriesMutatorFunc) []LabradarSeriesMutatorFunc {
	mutators := make([]LabradarSeriesMutatorFunc, len(first)+len(second))
	index := 0

	for i := 0; i < len(first); i++ {
		mutators[index] = first[i]
		index++
	}

	for i := 0; i < len(second); i++ {
		mutators[index] = second[i]
		index++
	}

	return mutators
}
