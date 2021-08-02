package labradar

import (
	"opgenorth.net/labradar/util"
)

type Velocities struct {
	Average            int     `json:"average"`
	Max                int     `json:"max"`
	Min                int     `json:"min"`
	ExtremeSpread      int     `json:"extremeSpread"`
	StandardDeviation  float64 `json:"standardDeviation"`
	VelocitiesInSeries []int   `json:"velocities"`
}

func (stats *Velocities) AddVelocity(velocity int) {
	stats.VelocitiesInSeries = append(stats.VelocitiesInSeries, velocity)
	min, max := util.GetMaxAndMin(stats.VelocitiesInSeries)

	stats.Average = int(util.CalculateAverage(stats.VelocitiesInSeries))
	stats.Max = max
	stats.Min = min
	stats.ExtremeSpread = max - min
	stats.StandardDeviation = util.CalculateStandardDeviation(stats.VelocitiesInSeries)
}
