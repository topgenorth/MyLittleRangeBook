package fs

import (
	"bufio"
	"encoding/json"
	"fmt"
	"github.com/spf13/afero"
	jww "github.com/spf13/jwalterweatherman"
	"io/ioutil"
	"opgenorth.net/labradar/labradar"
	"opgenorth.net/labradar/util"
	"os"
)

const DirectoryForJson = "/Users/tom/work/topgenorth.github.io/data/labradar/"

func LoadLabradarSeriesFromCsv(seriesNumber int, ls *labradar.Series) error {
	filename := util.GetPathToLabradarSeries(seriesNumber)
	a := afero.Afero{
		Fs: afero.NewOsFs(),
	}

	file, err := a.Open(filename)
	defer func(f afero.File) {
		err := f.Close()
		if err != nil {
			jww.ERROR.Println(err)
		}
	}(file)
	if err != nil {
		return err
	}

	skanner := bufio.NewScanner(file)
	var lineNumber = 0
	for skanner.Scan() {
		lineOfData := labradar.CreateLine(lineNumber, skanner.Text())
		ls.ParseLine(lineOfData)
		lineNumber++
	}

	if err := skanner.Err(); err != nil {
		return err
	}

	return nil
}

func SaveLabradarSeriesToJson(ls *labradar.Series) error {

	filename := ls.JsonFileName(DirectoryForJson)
	a := afero.Afero{
		Fs: afero.NewOsFs(),
	}

	exists, _ := a.Exists(filename)
	if exists {
		e1 := os.Remove(filename)
		if e1 != nil {
			jww.ERROR.Println(e1)
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

	jww.INFO.Printf("Saved the JSON to %s.", filename)

	fmt.Println(string(jsonToSave))
	return nil
}
