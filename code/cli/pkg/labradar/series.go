package labradar

import (
	"encoding/json"
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/context"
	"os"
	"sort"
	"text/template"
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

func (s Series) Print() {

	// TODO Inject some kind of printer thingy.

	t, err := template.New("Series").Parse(tmpl)
	if err != nil {
		panic(err)
	}

	err = t.Execute(os.Stdout, s)
	if err != nil {
		panic(err)
	}

}

func (s Series) TotalNumberOfShots() int {
	return len(s.Velocities.Values)
}

func (s Series) ToJsonBytes() []byte {
	s.RawData = sortRawDataByKey(s.RawData)
	jsonBytes, err := json.MarshalIndent(s, "", "  ")
	if err != nil {
		return nil
	}

	return jsonBytes
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

// Used to sort the lines of data in a series by their key, i.e the line number.
func sortRawDataByKey(d map[int]*LineOfData) map[int]*LineOfData {
	keys := make([]int, 0, len(d))
	for k := range d {
		keys = append(keys, k)
	}
	sort.Ints(keys)

	m := make(map[int]*LineOfData)
	for _, k := range keys {
		m[k] = d[k]
	}
	return m
}

const tmpl = `----
Labradar Series {{.Labradar.SeriesName}}

Number of Shots: {{.TotalNumberOfShots}}
Average Velocity: {{.Velocities.Average}}{{.Labradar.Units.Velocity}}
Standard Deviation: {{.Velocities.StandardDeviation}}{{.Labradar.Units.Velocity}}
Extreme Spread: {{.Velocities.ExtremeSpread}}{{.Labradar.Units.Velocity}}
----
`



