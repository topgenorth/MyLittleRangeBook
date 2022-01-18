package labradar

import (
	"encoding/json"
	"fmt"
	//"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"path"
)

//type OldDevice struct {
//	DeviceId   string                 `json:"deviceId"`
//	Date       string                 `json:"date"`
//	Time       string                 `json:"time"`
//	TimeZone   string                 `json:"timezone"`
//	SeriesName string                 `json:"seriesName"`
//	Units      *series.UnitsOfMeasure `json:"units"`
//}
//
//func (t OldDevice) String() string {
//	return t.DeviceId
//}

// OldSeries is replaced by the LabradarSeries structure.
type OldSeries struct {
	Number int `json:"number"`
	//Labradar   *OldDevice           `json:"labradar"`
	//Velocities *series.VelocityData   `json:"velocities"`
	//Firearm    *series.Firearm        `json:"firearm"`
	//LoadData   *series.LoadData       `json:"loadData"`
	Notes   string                 `json:"notes"`
	RawData map[int]*OldLineOfData `json:"data"`
}

// Filename infer the filename withing the LBR folder.
func (s *OldSeries) Filename() string {

	stub := fmt.Sprintf("%04d", s.Number)
	//goland:noinspection SpellCheckingInspection
	subdir := fmt.Sprintf("SR%s", stub)
	filename := fmt.Sprintf("SR%s Report.csv", stub)
	p := path.Join(subdir, filename)
	return p
}

func (s OldSeries) TotalNumberOfShots() int {
	return -1
	//return len(s.Velocities.Values)
}

func (s OldSeries) ToJsonBytes() []byte {
	jsonBytes, err := json.MarshalIndent(SortLinesOfData(s.RawData), "", "  ")
	if err != nil {
		return nil
	}

	return jsonBytes
}

func (s OldSeries) ToJson() (string, error) {
	jsonBytes, err := json.MarshalIndent(SortLinesOfData(s.RawData), "", "  ")
	if err != nil {
		return "", err
	}

	return fmt.Sprintf("%x", jsonBytes), nil
}

func (s *OldSeries) SetProjectile(projectileDescription string) {
	//if len(projectileDescription) > 0 {
	//	s.LoadData.Projectile = parseProjectileString(projectileDescription)
	//}
}

func (s *OldSeries) SetPowder(powderDescription string) {
	if len(powderDescription) > 0 {
		//s.LoadData.Powder = parsePowderString(powderDescription)
	}
}

type OldSeriesBuilder struct {
	*OldSeries
	RawData map[int]*OldLineOfData `json:"data"`
}

func (sb *OldSeriesBuilder) ParseLine(ld *OldLineOfData) {
	switch ld.LineNumber {
	case 1:
		sb.RawData[ld.LineNumber] = ld
		//sb.Labradar.DeviceId = ld.StringValue()
	case 3:
		sb.OldSeries.Number = ld.IntValue()
		//sb.Labradar.SeriesName = "SR" + ld.StringValue()
		sb.RawData[ld.LineNumber] = ld
	case 6:
		sb.RawData[ld.LineNumber] = ld
		//sb.Labradar.Units.Velocity = ld.StringValue()
	case 7:
		sb.RawData[ld.LineNumber] = ld
		//sb.Labradar.Units.Distance = ld.StringValue()
	case 9:
		sb.RawData[ld.LineNumber] = ld
		//sb.Labradar.Units.Weight = ld.StringValue()
	case 18:
		// For now, we only care about V0 (i.e. the muzzle velocity).
		sb.RawData[ld.LineNumber] = ld
		//sb.Velocities.AddVelocity(ld.IntValue())

		// We also pull the date and time from the first shot recorded
		//sb.Labradar.Date, sb.Labradar.Time = ld.DateAndTime()

	default:
		if ld.LineNumber > 18 {
			sb.RawData[ld.LineNumber] = ld
			//sb.Velocities.AddVelocity(ld.IntValue())
		}
	}
}
