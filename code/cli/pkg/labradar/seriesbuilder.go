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

	// TODO Don't rely on the line number to figure out what it is we're parsing.
	switch ld.LineNumber {
	case 1:
		sb.RawData[ld.LineNumber] = ld
		sb.Labradar.DeviceId = ld.getStringValue()
	case 3:
		sb.Labradar.SeriesName = ld.getStringValue()
		sb.RawData[ld.LineNumber] = ld
	case 6:
		sb.RawData[ld.LineNumber] = ld
		sb.Labradar.Units.Velocity = ld.getStringValue()
	case 7:
		sb.RawData[ld.LineNumber] = ld
		sb.Labradar.Units.Distance = ld.getStringValue()
	case 9:
		sb.RawData[ld.LineNumber] = ld
		sb.Labradar.Units.Weight = ld.getStringValue()
	case 18:
		// For now, we only care about V0 (i.e. the muzzle velocity).
		sb.RawData[ld.LineNumber] = ld
		sb.Velocities.AddVelocity(ld.getIntValue())

		// We also pull the date and time from the first shot recorded
		sb.Labradar.Date, sb.Labradar.Time = ld.getDateAndTime()

	default:
		if ld.LineNumber > 18 {
			sb.RawData[ld.LineNumber] = ld
			sb.Velocities.AddVelocity(ld.getIntValue())
		}
	}
}
