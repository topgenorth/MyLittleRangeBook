package domain

import (
	"labreader/labradar"
)

const (
	SeriesLoadedEvent     = "series.SeriesLoaded"
	SeriesShotAddedEvent  = "series.ShotAdded"
	SeriesShotHiddenEvent = "series.ShotHidden"
)

type (
	SeriesLoaded struct {
		labradar.SeriesNumber
		labradar.FileDirectory
		Units string
	}
)
