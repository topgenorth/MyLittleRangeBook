package labradar

import "opgenorth.net/mylittlerangebook/pkg/util"

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
	min, max := util.GetMaxAndMin(stats.Values)

	stats.Average = int(util.CalculateAverage(stats.Values))
	stats.Max = max
	stats.Min = min
	stats.ExtremeSpread = max - min
	stats.StandardDeviation = util.CalculateStandardDeviation(stats.Values)
}
