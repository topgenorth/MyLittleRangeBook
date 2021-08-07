package labradar

import (
	"encoding/json"
	"sort"
)

type Series struct {
	Number   int                 `json:"number"`
	Labradar *Device             `json:"labradar"`
	Firearm  *Firearm            `json:"firearm"`
	LoadData *LoadData           `json:"loadData"`
	Notes    string              `json:"notes"`
	Tags     []string            `json:"tags"`
	RawData  map[int]*LineOfData `json:"data"`
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
