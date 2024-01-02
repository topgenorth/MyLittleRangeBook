package series

type SeriesCreated struct {
	Series *Series
}

func (SeriesCreated) EventName() string { return "series.SeriesCreated" }

type SeriesDeleted struct {
	Series *Series
}

func (SeriesDeleted) EventName() string { return "series.SeriesDeleted" }

type VelocityAdded struct {
	Number   int
	Velocity int
}

func (VelocityAdded) EventName() string { return "series.VelocityAdded" }

type VelocityDeleted struct {
	velocityData
	Number int
}

func (VelocityDeleted) EventName() string { return "series.VelocityDeleted" }
