package labradar

import (
	"bufio"
	"encoding/json"
	"fmt"
	"github.com/carolynvs/aferox"
	"opgenorth.net/mylittlerangebook/pkg/context"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"path"
	"time"
)

type OldDevice struct {
	DeviceId   string                 `json:"deviceId"`
	Date       string                 `json:"date"`
	Time       string                 `json:"time"`
	TimeZone   string                 `json:"timezone"`
	SeriesName string                 `json:"seriesName"`
	Units      *series.UnitsOfMeasure `json:"units"`
}

func (t OldDevice) String() string {
	return t.DeviceId
}

type OldSeries struct {
	Number     int                  `json:"number"`
	Labradar   *OldDevice           `json:"labradar"`
	Velocities *series.VelocityData `json:"velocities"`
	Firearm    *series.Firearm      `json:"firearm"`
	LoadData   *series.LoadData     `json:"loadData"`
	Notes      string               `json:"notes"`
	RawData    map[int]*LineOfData  `json:"data"`
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

func CreateOldSeries() *OldSeries {

	now := time.Now()

	u := &series.UnitsOfMeasure{
		Velocity: "fps",
		Distance: "m",
		Weight:   "gr (grains)",
	}
	d := &OldDevice{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		context.DefaultTimeZone,
		fmt.Sprintf("SR%04d", 0),
		u,
	}

	vd := &series.VelocityData{
		Average:           0,
		Max:               0,
		Min:               0,
		ExtremeSpread:     0,
		StandardDeviation: 0,
		Values:            nil,
	}
	f := &series.Firearm{
		Name:      "",
		Cartridge: "",
	}
	pr := &series.Projectile{
		Name:   "",
		Weight: 0,
		BC: &series.BallisticCoefficient{
			DragModel: "",
			Value:     0,
		},
	}
	po := &series.PowderCharge{
		Name:   "",
		Amount: 0,
	}
	ld := &series.LoadData{
		Cartridge:  "",
		Projectile: pr,
		Powder:     po,
	}
	ls := &OldSeries{
		Number:     0,
		Labradar:   d,
		Velocities: vd,
		Firearm:    f,
		LoadData:   ld,
		Notes:      "",
		RawData:    make(map[int]*LineOfData),
	}

	return ls
}

func (s OldSeries) TotalNumberOfShots() int {
	return len(s.Velocities.Values)
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
	if len(projectileDescription) > 0 {
		s.LoadData.Projectile = parseProjectileString(projectileDescription)
	}
}

func (s *OldSeries) SetPowder(powderDescription string) {
	if len(powderDescription) > 0 {
		s.LoadData.Powder = parsePowderString(powderDescription)
	}
}

func initDevice(seriesNumber int, timezone *time.Location) *OldDevice {
	now := time.Now().In(timezone)

	u := &series.UnitsOfMeasure{
		Velocity: "fps",
		Distance: "m",
		Weight:   "gr (grains)",
	}
	return &OldDevice{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		timezone.String(),
		fmt.Sprintf("SR%04d", seriesNumber),
		u,
	}
}

// LoadSeries will take the specified CSV file and return a OldSeries.
func LoadSeries(filename string, fs aferox.Aferox) (*OldSeries, error) {
	f, err := fs.Open(filename)
	if err != nil {
		return nil, fmt.Errorf("could not load the series at %s: %w", filename, err)
	}

	//defer CloseFile(f)

	builder := NewSeriesBuilder()
	scanner := bufio.NewScanner(f)
	var lineNumber = 0
	for scanner.Scan() {
		l := NewLineOfData(lineNumber, scanner.Text())
		builder.ParseLine(l)
		lineNumber++
	}

	return builder.OldSeries, nil
}

// SeriesWriter is an interface to persisting a OldSeries to something a person might read (HTML, JSON, Markdown, etc).
type SeriesWriter interface {
	Write(s OldSeries) error
}

type SeriesBuilder struct {
	*OldSeries
	RawData map[int]*LineOfData `json:"data"`
}

func NewSeriesBuilder() *SeriesBuilder {
	return &SeriesBuilder{
		CreateOldSeries(),
		make(map[int]*LineOfData),
	}
}

func (sb *SeriesBuilder) ParseLine(ld *LineOfData) {
	switch ld.LineNumber {
	case 1:
		sb.RawData[ld.LineNumber] = ld
		sb.Labradar.DeviceId = ld.StringValue()
	case 3:
		sb.OldSeries.Number = ld.IntValue()
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
