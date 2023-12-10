package domain

import (
	"labreader/internal/ddd"
	"labreader/labradar"
)

const SeriesAggregate = "labradar.SeriesAggregate"

type (
	Series struct {
		ddd.Aggregate
		DeviceId string
		labradar.SeriesNumber
		labradar.FileDirectory
	}

	Shot struct {
		Number         int
		MuzzleVelocity int
		Units          string
	}
)

func NewSeries(seriesNumber labradar.SeriesNumber, dir labradar.FileDirectory) *Series {

	return &Series{
		Aggregate: ddd.NewAggregate(seriesNumber.SeriesFile(), SeriesAggregate),
	}
}
