package labradar

type SeriesBuilder struct {
	*Series
	RawData map[int]*LineOfData `json:"data"`
}

func NewSeriesBuilder() *SeriesBuilder {
	return &SeriesBuilder{
		NewSeries(),
		make(map[int]*LineOfData),
	}
}

func (sb *SeriesBuilder) ParseLine(ld *LineOfData) {

	// TODO Don't rely on the line Number to figure out what it is we're parsing.
	switch ld.LineNumber {
	case 1:
		sb.RawData[ld.LineNumber] = ld
		sb.Labradar.DeviceId = ld.StringValue()
	case 3:
		sb.Series.Number = ld.IntValue()
		sb.Labradar.SeriesName = "SR" + ld.StringValue()
		sb.RawData[ld.LineNumber] = ld
	case 6:
		sb.RawData[ld.LineNumber] = ld
		sb.Labradar.Units.Velocity = ld.StringValue()
	case 7:
		sb.RawData[ld.LineNumber] = ld
		sb.Labradar.Units.Distance = ld.StringValue()
	case 9:
		sb.RawData[ld.LineNumber] = ld
		sb.Labradar.Units.Weight = ld.StringValue()
	case 18:
		// For now, we only care about V0 (i.e. the muzzle velocity).
		sb.RawData[ld.LineNumber] = ld
		sb.Velocities.AddVelocity(ld.IntValue())

		// We also pull the date and time from the first shot recorded
		sb.Labradar.Date, sb.Labradar.Time = ld.DateAndTime()

	default:
		if ld.LineNumber > 18 {
			sb.RawData[ld.LineNumber] = ld
			sb.Velocities.AddVelocity(ld.IntValue())
		}
	}
}
