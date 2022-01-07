package labradar

import (
	"encoding/json"
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/context"
	"strconv"
	"strings"
	"time"
)

type Series struct {
	Number     int                 `json:"number"`
	Labradar   *Device             `json:"labradar"`
	Velocities *VelocityData       `json:"velocities"`
	Firearm    *Firearm            `json:"firearm"`
	LoadData   *LoadData           `json:"loadData"`
	Notes      string              `json:"notes"`
	RawData    map[int]*LineOfData `json:"data"`
}

type SeriesError struct {
	Msg    string
	Number int
}

func (s SeriesError) Error() string {
	return fmt.Sprintf("There was a problem trying to process series %d: %s.", s.Number, s.Msg)
}

func NewSeries() *Series {

	now := time.Now()

	u := &UnitsOfMeasure{
		Velocity: "fps",
		Distance: "m",
		Weight:   "gr (grains)",
	}
	d := &Device{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		context.DefaultTimeZone,
		fmt.Sprintf("SR%04d", 0),
		u,
	}

	vd := &VelocityData{
		Average:           0,
		Max:               0,
		Min:               0,
		ExtremeSpread:     0,
		StandardDeviation: 0,
		Values:            nil,
	}
	f := &Firearm{
		Name:      "",
		Cartridge: "",
	}
	pr := &Projectile{
		Name:   "",
		Weight: 0,
		BC: &BallisticCoefficient{
			DragModel: "",
			Value:     0,
		},
	}
	po := &PowderCharge{
		Name:   "",
		Amount: 0,
	}
	ld := &LoadData{
		Cartridge:  "",
		Projectile: pr,
		Powder:     po,
	}
	ls := &Series{
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

func (s Series) TotalNumberOfShots() int {
	return len(s.Velocities.Values)
}

func (s Series) ToJsonBytes() []byte {
	jsonBytes, err := json.MarshalIndent(SortLinesOfData(s.RawData), "", "  ")
	if err != nil {
		return nil
	}

	return jsonBytes
}

func (s Series) ToJson() (string, error) {
	jsonBytes, err := json.MarshalIndent(SortLinesOfData(s.RawData), "", "  ")
	if err != nil {
		return "", err
	}

	return fmt.Sprintf("%x", jsonBytes), nil
}

func (s *Series) SetProjectile(projectileDescription string) {
	if len(projectileDescription) > 0 {
		s.LoadData.Projectile = parseProjectileString(projectileDescription)
	}
}

func (s *Series) SetPowder(powderDescription string) {
	if len(powderDescription) > 0 {
		s.LoadData.Powder = parsePowderString(powderDescription)
	}
}

func parsePowderString(powder string) *PowderCharge {
	parts := RemoveEmptyStrings(strings.Split(powder, " "))
	if len(parts) < 1 {
		return &PowderCharge{Name: "Unknown", Amount: 0.0}
	}

	p := &PowderCharge{
		Name:   parseNameOfProjectileFromString(strings.Join(parts[1:], " ")),
		Amount: parseAmountFromPowderString(parts[0]),
	}
	return p
}

func parseProjectileString(projectile string) *Projectile {
	parts := RemoveEmptyStrings(strings.Split(projectile, " "))

	if len(parts) < 1 {
		return &Projectile{Name: "Unknown", Weight: 0, BC: nil}
	}

	p := &Projectile{
		Name:   parseNameOfProjectileFromString(strings.Join(parts[1:], " ")),
		Weight: parseWeightFromProjectileString(parts[0]),
		BC:     nil, // [TO20220106] We don't worry about BC right now.
	}

	return p
}

func parseNameOfProjectileFromString(name string) string {

	replacer := strings.NewReplacer(
		"grains", "",
		"grain", "",
		"gr.", "",
		"gr", "",
	)
	return strings.TrimSpace(replacer.Replace(name))
}

func parseAmountFromPowderString(amount string) float32 {

	replacer := strings.NewReplacer(
		"grains", "",
		"grain", "",
		"gr.", "",
		"gr", "",
	)

	str := strings.TrimSpace(replacer.Replace(amount))

	w, err := strconv.ParseFloat(str, 32)
	if err != nil {
		return 0.0
	}

	return float32(w)
}

func parseWeightFromProjectileString(weight string) int {

	replacer := strings.NewReplacer(
		"grains", "",
		"grain", "",
		"gr.", "",
		"gr", "",
	)

	str := strings.TrimSpace(replacer.Replace(weight))

	w, err := strconv.ParseFloat(str, 32)
	if err != nil {
		return 0
	}

	return int(w)
}

func initDevice(seriesNumber int, timezone *time.Location) *Device {
	now := time.Now().In(timezone)

	u := &UnitsOfMeasure{
		Velocity: "fps",
		Distance: "m",
		Weight:   "gr (grains)",
	}
	return &Device{
		"",
		now.Format("YYYY-MM-DD"),
		now.Format("15:04"),
		timezone.String(),
		fmt.Sprintf("SR%04d", seriesNumber),
		u,
	}
}
