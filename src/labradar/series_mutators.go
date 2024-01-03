package labradar

import "time"

// WithSeriesNumber will use an integer to initialize the number assigned by a specific Labradar device
func WithSeriesNumber(n int) SeriesMutatorFn {
	return func(s *Series) {
		s.number = SeriesNumber(n)
	}
}

// WithNotes will update the notes field.
func WithNotes(notes string) SeriesMutatorFn {
	return func(s *Series) {
		//s.Notes = notes
	}
}

func AppendVelocity(velocity int) SeriesMutatorFn {
	return func(s *Series) {
		if velocity < 1 {
			panic("velocity was too low")
		}
		s.velocities.Append(velocity)
	}
}

// UsingGrainsForWeight sets the units of measure to grains.
func UsingGrainsForWeight() SeriesMutatorFn {
	return func(s *Series) {
		s.unitsOfMeasure.Weight = "gr"
	}
}

// UsingCurrentDateAndTime will use UTC from this process as the time..
func UsingCurrentDateAndTime() SeriesMutatorFn {
	return func(s *Series) {
		s.dateTime = time.Now().UTC()

	}
}

// UsingFeetPerSecondForMuzzleVelocity will set the default velocity units to FPS
func UsingFeetPerSecondForMuzzleVelocity() SeriesMutatorFn {
	return func(s *Series) {
		s.unitsOfMeasure.Velocity = "fps"
	}
}

// UsingMetresPerSecondForMuzzleVelocity will set the default velocity units to m/s
func UsingMetresPerSecondForMuzzleVelocity() SeriesMutatorFn {
	return func(s *Series) {
		s.unitsOfMeasure.Velocity = "m/s"
	}
}

// UsingYardsForDistance will set the default distance units to yards.
func UsingYardsForDistance() SeriesMutatorFn {
	return func(s *Series) {
		s.unitsOfMeasure.Distance = "y"
	}
}

// UsingMetresForDistance will set the default distance units to metres.
func UsingMetresForDistance() SeriesMutatorFn {
	return func(s *Series) {
		s.unitsOfMeasure.Distance = "m"
	}
}

// UsingFeetForDistance will set the default distance units to feet.
func UsingFeetForDistance() SeriesMutatorFn {
	return func(s *Series) {
		s.unitsOfMeasure.Distance = "ft"
	}
}

// LabradarSeriesDefaults returns the mutators that will set default values on a Series.
func LabradarSeriesDefaults() []SeriesMutatorFn {
	defaults := []SeriesMutatorFn{
		UsingGrainsForWeight(),
		UsingMetresForDistance(),
		UsingFeetPerSecondForMuzzleVelocity(),
		UsingCurrentDateAndTime(),
		UsingCelsiusForTemperature(),
	}

	return defaults
}

// UsingCelsiusForTemperature will return a mutator to set the temperature Units of Measure to Centigrade.
func UsingCelsiusForTemperature() SeriesMutatorFn {
	return func(s *Series) {
		s.unitsOfMeasure.Temperature = "Celsius"
	}
}

// UsingFarenheitForTemperature will return a mutator to set the temperature Units of Measure to Fahrenheit.
func UsingFarenheitForTemperature() SeriesMutatorFn {
	return func(s *Series) {
		s.unitsOfMeasure.Temperature = "Fahrenheit"
	}
}

// combineMutators will combine two separate arrays of SeriesMutatorFn into one.  The items in the first
// array will appear first.
func combineMutators(first []SeriesMutatorFn, second []SeriesMutatorFn) []SeriesMutatorFn {
	mutators := make([]SeriesMutatorFn, len(first)+len(second))
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
