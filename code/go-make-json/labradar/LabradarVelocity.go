package labradar

import "math"

type LabradarVelocity struct {
	Average            int
	Max                int
	Min                int
	ExtremeSpread      int
	StandardDeviation  float64
	VelocitiesInSeries []int
}

func (stats *LabradarVelocity) addVelocity(velocity int) {
	stats.VelocitiesInSeries = append(stats.VelocitiesInSeries, velocity)
	min, max := getMaxAndMin(stats.VelocitiesInSeries)

	stats.Average = int(average(stats.VelocitiesInSeries))
	stats.Max = max
	stats.Min = min
	stats.ExtremeSpread = max - min
	stats.StandardDeviation = standardDeviation(stats.VelocitiesInSeries)
}

func getMaxAndMin(array []int) (int, int) {
	max := array[0]
	min := array[0]
	for _, value := range array {
		if max < value {
			max = value
		}
		if min > value {
			min = value
		}
	}
	return min, max
}

func average(array []int) float64 {
	count := len(array)
	sum := 0
	for _, velocity := range array {
		sum += velocity
	}

	return float64(sum) / float64(count)
}

func standardDeviation(array []int) float64 {
	count := len(array)
	mean := average(array)
	var sd float64
	sd = float64(0)
	for j := 0; j < count; j++ {
		sd += math.Pow(float64(array[j])-mean, 2)
	}

	return math.Sqrt(sd / float64(count))
}
