package labradar

import (
	"encoding/json"
	"opgenorth.net/labradar/pkg/util"
	"sort"
)

type BallisticCoefficient struct {
	DragModel string  `json:"dragModel"`
	Value     float32 `json:"value"`
}

type LoadData struct {
	Cartridge  string        `json:"cartridge"`
	Projectile *Projectile   `json:"projectile"`
	Powder     *PowderCharge `json:"powder"`
}

type Device struct {
	DeviceId   string          `json:"deviceId"`
	Date       string          `json:"date"`
	Time       string          `json:"time"`
	TimeZone   string          `json:"timezone"`
	SeriesName string          `json:"seriesName"`
	Units      *UnitsOfMeasure `json:"units"`
	Stats      *Velocities     `json:"stats"`
}

type Firearm struct {
	Name      string `json:"name"`
	Cartridge string `json:"cartridge"`
}

type Projectile struct {
	Name   string                `json:"name"`
	Weight int                   `json:"weight"`
	BC     *BallisticCoefficient `json:"bc"`
}

type PowderCharge struct {
	Name   string  `json:"name"`
	Amount float32 `json:"amount"`
}

type Series struct {
	Number   int                 `json:"number"`
	Labradar *Device             `json:"labradar"`
	Firearm  *Firearm            `json:"firearm"`
	LoadData *LoadData           `json:"loadData"`
	Notes    string              `json:"notes"`
	Tags     []string            `json:"tags"`
	RawData  map[int]*LineOfData `json:"data"`
}

type UnitsOfMeasure struct {
	Velocity string `json:"velocity"`
	Distance string `json:"distance"`
	Weight   string `json:"weight"`
}

type Velocities struct {
	Average            int     `json:"average"`
	Max                int     `json:"max"`
	Min                int     `json:"min"`
	ExtremeSpread      int     `json:"extremeSpread"`
	StandardDeviation  float64 `json:"standardDeviation"`
	VelocitiesInSeries []int   `json:"velocities"`
}

func (stats *Velocities) AddVelocity(velocity int) {
	stats.VelocitiesInSeries = append(stats.VelocitiesInSeries, velocity)
	min, max := util.GetMaxAndMin(stats.VelocitiesInSeries)

	stats.Average = int(util.CalculateAverage(stats.VelocitiesInSeries))
	stats.Max = max
	stats.Min = min
	stats.ExtremeSpread = max - min
	stats.StandardDeviation = util.CalculateStandardDeviation(stats.VelocitiesInSeries)
}

func (ls *Series) TotalNumberOfShots() int {
	return len(ls.Labradar.Stats.VelocitiesInSeries)
}

func (ls *Series) ToJson() []byte {
	ls.RawData = sortRawDataByKey(ls.RawData)
	jsonBytes, err := json.MarshalIndent(ls, "", "  ")
	if err != nil {
		return nil
	}

	return jsonBytes
}

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