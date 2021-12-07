package labradar

import (
	"encoding/json"
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/context"
	"os"
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
	t, err := template.New("Series").Parse(tmpl_summarize_series)
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

func (s Series) SaveDescription() error {
	t, err := template.New("DescribeSeries").Parse(tmpl_describe_series)
	if err != nil {
		return fmt.Errorf("SaveDescription - %v", err)
	}

	err = t.Execute(os.Stdout, s)
	if err != nil {
		return fmt.Errorf("SaveDescription - %v", err)
	}
	return nil
}

//func (s Series) SaveTo(cfg *ObsoleteLabradarCsvFile) error {
//	outputFileName := filepath.Join(cfg.OutputDir, s.Labradar.SeriesName+".json")
//
//	exists, err := cfg.FileSystem.Exists(outputFileName)
//	if err != nil {
//		return err
//	}
//
//	if exists {
//		err := os.Remove(outputFileName)
//		if err != nil {
//			return err
//		}
//	}
//
//	err = cfg.FileSystem.WriteFile(outputFileName, s.ToJsonBytes(), 0644)
//	if err != nil {
//		return err
//	}
//
//	return nil
//}

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

const tmpl_summarize_series = `----
Labradar Series {{.Labradar.SeriesName}}

Number of Shots: {{.TotalNumberOfShots}}
Average Velocity: {{.Velocities.Average}}{{.Labradar.Units.Velocity}}
Standard Deviation: {{.Velocities.StandardDeviation}}{{.Labradar.Units.Velocity}}
Extreme Spread: {{.Velocities.ExtremeSpread}}{{.Labradar.Units.Velocity}}
----
`

const tmpl_describe_series = `
# Description of Labradar series

For ammo, stick with the format:

Cartridge; Bullet; Powder; COAL or CBTO

| Series Number | Ammo | Firearm | Date | 
| :---:         | :--- | :-----  | :---: |
| {{.Number}} | {{.Notes}} | {{.Firearm.Name}} | {{.Labradar.Date}} |
`
