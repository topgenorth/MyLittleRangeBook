package labradar

import (
	"github.com/carolynvs/aferox"
	"os"
	"path/filepath"
)

type SeriesBuilder struct {
	*Series
	RawData map[int]*LineOfData `json:"data"`
}

func NewSeriesBuilder() *SeriesBuilder {
	return &SeriesBuilder{
		NewSeries2(),
		make(map[int]*LineOfData),
	}
}

func (sb *SeriesBuilder) ParseLine(ld *LineOfData) {
	switch ld.LineNumber {
	case 1:
		sb.RawData[ld.LineNumber] = ld
		sb.Labradar.DeviceId = ld.getStringValue()
	case 3:
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

func (sb *SeriesBuilder) SaveToJson(cfg *ReadCsvConfig) error {
	outputFileName := filepath.Join(cfg.OutputDir, sb.Labradar.SeriesName+".json")

	err := deleteFileIfExists(cfg.FileSystem, outputFileName)
	if err != nil {
		return err
	}

	err2 := cfg.FileSystem.WriteFile(outputFileName, sb.ToJson(), 0644)
	if err2 != nil {
		return err2
	}

	return nil
}

func deleteFileIfExists(a aferox.Aferox, fileName string) error {
	exists, _ := a.Exists(fileName)
	if exists {
		err := os.Remove(fileName)
		if err != nil {
			return err
		}
	}
	return nil
}
