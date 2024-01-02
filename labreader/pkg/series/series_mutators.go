package series

// combineMutators will combine two separate arrays of MutatorFunction into one.  The items in the first
// array will appear first.
func combineMutators(first []MutatorFunction, second []MutatorFunction) []MutatorFunction {
	mutators := make([]MutatorFunction, len(first)+len(second))
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

// AddMuzzleVelocitiesFromCsv will read all the muzzle velocities from the CSV file.
