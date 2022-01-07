package labradar

import (
	"opgenorth.net/mylittlerangebook/pkg/math"
)

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
