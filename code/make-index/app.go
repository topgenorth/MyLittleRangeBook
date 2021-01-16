package main

import (
	bytes "bytes"
	"fmt"
	"io/ioutil"
	"os"
	"path"
	"path/filepath"
	//log "github.com/sirupsen/logrus"
)

const LabradarDataFiles = "../../LBR/"

type LabradarFile struct {
	SeriesNumber int
	SeriesName string
	ReportFileName string
	FullName string
}
func (lr*LabradarFile) RemoveNullCharacters(newDirectory string) (string, error) {
	originalBytes, err := ioutil.ReadFile(lr.FullName)
	if err != nil {
		return "", err
	}

	if  !bytes.ContainsAny(originalBytes, "\x00") {
		return "", nil
	}

	newFile:= path.Join(newDirectory, fmt.Sprintf("%s.csv",lr.SeriesName))
	fileContentsWithoutNull := bytes.ReplaceAll(originalBytes, []byte {'\x00'}, []byte {})
	err = ioutil.WriteFile(newFile, fileContentsWithoutNull, 0644)
	if err == nil {
		return newFile, nil
	} else  {
		return newFile, err
	}
}

func  NewLabradarFile(seriesNumber int, lbrDirectory string ) *LabradarFile {
	seriesName := fmt.Sprintf("SR%04d", seriesNumber)
	reportFilename := fmt.Sprintf("%s/%s Report.csv",seriesName,  seriesName)
	return &LabradarFile{ SeriesNumber: seriesNumber,
		SeriesName: seriesName,
		ReportFileName: reportFilename,
		FullName: fmt.Sprintf("%s/%s", lbrDirectory, reportFilename ),
	}
}

func (f*LabradarFile) Exists(lbrDirectory string) bool {
	s := filepath.Join(lbrDirectory, f.ReportFileName)
	_, err :=  os.Stat(s)
	return err == nil
}

func main() {
	workingDir, _ := os.Getwd()
	lbrDirectory  := path.Join(workingDir , LabradarDataFiles)

	lrFile := NewLabradarFile(1, lbrDirectory )
	if !lrFile.Exists(LabradarDataFiles) {
		fmt.Printf("The file `%s` does not exist.\n", lrFile.FullName)
		return
	}

	fmt.Printf("Processing the file `%s`.\n", lrFile.FullName)

	newFile, err := lrFile.RemoveNullCharacters(lbrDirectory)
	if err !=nil {
		fmt.Printf("Could not remove null characters from %s (destination %s)\n", lrFile.FullName, newFile)
	} else {
		fmt.Printf("File with NULL characters removed is at %s.\n", newFile)
	}
	fmt.Println("Done")
}
