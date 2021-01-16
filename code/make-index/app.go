package main

import (
	"fmt"
	"os"
	"path/filepath"
)

const LABRADAR_DATA_FILES = "../../LBR/"

func getReportFileForSeries(seriesName string) (string, error) {
	path, _ := os.Getwd()
	csvFile := filepath.Join(path, LABRADAR_DATA_FILES, seriesName, fmt.Sprintf("%s Report.csv", seriesName))
	_, err := os.Stat(csvFile)

	return csvFile, err
}

func main() {
	csvFile, err := getReportFileForSeries("SR0001")
	if err != nil {
		fmt.Printf("The file %s is not valid. %s", csvFile, err)
		return
	}
	fmt.Printf("Processing the file %s.", csvFile)
	fmt.Println("")
}
