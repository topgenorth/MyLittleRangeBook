package labradar

import (
	"bufio"
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/labradar/pkg/config"
	"os"
	"path/filepath"
	"strconv"
	"strings"
)

type LineOfData struct {
	LineNumber int    `json:"lineNumber"`
	Raw        string `json:"raw"`
	Value      string `json:"value"`
}

func CreateLine(linenumber int, s string) *LineOfData {
	return &LineOfData{
		LineNumber: linenumber,
		Raw:        s,
		Value:      fixupLabradarLine(s),
	}
}

func (ld *LineOfData) getStringValue() string {
	parts := strings.Split(ld.Value, ";")
	if len(parts) < 2 {
		return ""
	}
	return parts[1]
}

func (ld *LineOfData) getIntValue() int {
	parts := strings.Split(ld.Value, ";")
	if len(parts) < 2 {
		return -1
	}
	i, _ := strconv.Atoi(parts[1])
	return i
}

func (ld *LineOfData) getDateAndTime() (string, string) {
	parts := strings.Split(ld.Value, ";")
	l := len(parts)
	if l == 1 {
		return "", ""
	}
	return parts[l-3], parts[l-2]
}

func closeFile(f afero.File) {
	err := f.Close()
	if err != nil {
		fmt.Println(err)
	}
}

func openFile(filename string, a afero.Afero) (afero.File, error) {
	file, err := a.Open(filename)
	if err != nil {
		return nil, err
	}

	return file, nil
}

func LoadLabradarSeriesFromCsv(ls *Series, cfg *config.Config) error {
	inputFileName := filepath.Join(cfg.InputDir, ls.Labradar.SeriesName, ls.Labradar.SeriesName+" Report.csv")

	file, err := openFile(inputFileName, cfg.Context.Afero)
	if err != nil {
		fmt.Println("Could not open the file " + inputFileName)
		return err
	}
	defer closeFile(file)

	s := bufio.NewScanner(file)
	var lineNumber = 0
	for s.Scan() {
		lineOfData := CreateLine(lineNumber, s.Text())
		ls.parseLineOfTextFromLabradarCsv(lineOfData)
		lineNumber++
	}

	if err := s.Err(); err != nil {
		return err
	}

	return nil
}

func SaveLabradarSeriesToJson(ls *Series, cfg *config.Config) error {
	outputFileName := filepath.Join(cfg.OutputDir, ls.Labradar.SeriesName+".json")

	err := deleteFileIfExists(cfg.Context.Afero, outputFileName)
	if err != nil {
		return err
	}

	err2 := cfg.Context.Afero.WriteFile(outputFileName, ls.ToJson(), 0644)
	if err2 != nil {
		return err2
	}

	return nil
}

func isLabradarCsvFile(path string) bool {
	b := strings.ToLower(filepath.Base(path))
	ext := filepath.Ext(b)
	if ext == ".csv" {
		sr := b[0:2]
		if sr == "sr" {
			return true
		}
	}

	return false
}

func GetLabradarFilesInDir(dirName string) []string {
	var filenames []string

	err := filepath.Walk(dirName, func(path string, info os.FileInfo, err error) error {
		if !info.IsDir() && isLabradarCsvFile(path) {
			filenames = append(filenames, path)
		}
		return nil
	})
	if err != nil {
		panic(err)
	}

	return filenames
}

func deleteFileIfExists(a afero.Afero, fileName string) error {
	exists, _ := a.Exists(fileName)
	if exists {
		err := os.Remove(fileName)
		if err != nil {
			return err
		}
	}
	return nil
}
