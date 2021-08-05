package labradar

import (
	"bufio"
	"encoding/json"
	"fmt"
	"github.com/spf13/afero"
	"io/ioutil"
	"os"
)

type Series struct {
	Number   int                 `json:"number"`
	Labradar *Labradar           `json:"labradar"`
	Firearm  *Firearm            `json:"firearm"`
	LoadData *LoadData           `json:"loadData"`
	Notes    string              `json:"notes"`
	Tags     []string            `json:"tags"`
	Data     map[int]*LineOfData `json:"data"`
}

func (ls *Series) TotalNumberOfShots() int {
	return len(ls.Labradar.Stats.VelocitiesInSeries)
}

func NewSeries() *Series {

	ls := &Series{
		Number:   0,
		Labradar: initLabradarStruct(0),
		Firearm: &Firearm{
			Name:      "",
			Cartridge: "",
		},
		LoadData: &LoadData{
			Cartridge: "",
			Projectile: &Projectile{
				Name:   "",
				Weight: 0,
				BC: &BallisticCoefficient{
					DragModel: "",
					Value:     0,
				},
			},
			Powder: &PowderCharge{
				Name:   "",
				Amount: 0,
			},
		},
		Notes: "",
		Tags:  make([]string, 10), // making a wild guess at how many tags we'll need.
		Data:  make(map[int]*LineOfData),
	}

	return ls
}

func LoadLabradarSeriesFromCsv(seriesNumber int, ls *Series) error {
	filename, err := getPathToLabradarCsvFile(seriesNumber)
	a := afero.Afero{
		Fs: afero.NewOsFs(),
	}

	file, err := a.Open(filename)
	defer func(f afero.File) {
		err := f.Close()
		if err != nil {
			fmt.Println(err)
		}
	}(file)
	if err != nil {
		return err
	}

	skanner := bufio.NewScanner(file)
	var lineNumber = 0
	for skanner.Scan() {
		lineOfData := CreateLine(lineNumber, skanner.Text())
		ls.parseLine(lineOfData)
		lineNumber++
	}

	if err := skanner.Err(); err != nil {
		return err
	}

	return nil
}

func SaveLabradarSeriesToJson(ls *Series) error {

	filename := ls.jsonFileName(DirectoryForJson)
	a := afero.Afero{
		Fs: afero.NewOsFs(),
	}

	exists, _ := a.Exists(filename)
	if exists {
		e1 := os.Remove(filename)
		if e1 != nil {
			return e1
		}
	}

	jsonToSave, err := json.MarshalIndent(ls, "", "  ")
	if err != nil {
		return err
	}

	err2 := ioutil.WriteFile(filename, jsonToSave, 0644)
	if err2 != nil {
		return err2
	}

	return nil
}

