package labradar

import "opgenorth.net/mylittlerangebook/pkg/labradar/series"

func LabradarSeriesDefaults() []series.LabradarSeriesMutatorFunc {
	defaults := []series.LabradarSeriesMutatorFunc{
		series.UsingGrainsForWeight(),
		series.UsingMetresForDistance(),
		series.UsingFeetPerSecondForMuzzleVelocity(),
		series.UsingCurrentDateAndTime(),
	}

	return defaults
}

// MergeMutators will combine two separate arrays of LabradarSeriesMutatorFunc into one.  The items in the first
// array will appear first.
func MergeMutators(first []series.LabradarSeriesMutatorFunc, second []series.LabradarSeriesMutatorFunc) []series.LabradarSeriesMutatorFunc {
	mutators := make([]series.LabradarSeriesMutatorFunc, len(first)+len(second))
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
