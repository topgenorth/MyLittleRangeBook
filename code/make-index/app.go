package main

import (
	"bytes"
	"fmt"
	"github.com/rs/zerolog"
	"github.com/rs/zerolog/log"
	"io/ioutil"
	"os"
	"path"
	"path/filepath"
)

const LabradarDataFiles = "../../LBR/"

type LabradarFile struct {
	SeriesNumber   int
	SeriesName     string
	ReportFileName string
	FullName       string
}

func main() {
	zerolog.TimeFieldFormat = zerolog.TimeFormatUnix
	zerolog.SetGlobalLevel(zerolog.TraceLevel)
	log.Logger = log.Output(zerolog.ConsoleWriter{Out: os.Stderr})

	workingDir, _ := os.Getwd()
	lbrDirectory := path.Join(workingDir, LabradarDataFiles)

	lrFile := NewLabradarFile(1, lbrDirectory)
	if !lrFile.exists(LabradarDataFiles) {
		log.Error().Msgf("The file `%s` does not exist.\n", lrFile.FullName)
		return
	}

	log.Debug().Msgf("Processing the file `%s`.", lrFile.FullName)

	newFile, err := lrFile.removeNullCharacters(lbrDirectory)
	if err != nil {
		log.Warn().Msgf("Could not remove null characters from %s (destination %s)\n", lrFile.FullName, newFile)
	} else {
		log.Info().Msgf("NULL characters removed from file %s.", newFile)
	}
	fmt.Println("Done")
}

func NewLabradarFile(seriesNumber int, lbrDirectory string) *LabradarFile {
	seriesName := fmt.Sprintf("SR%04d", seriesNumber)
	reportFilename := fmt.Sprintf("%s/%s Report.csv", seriesName, seriesName)
	return &LabradarFile{SeriesNumber: seriesNumber,
		SeriesName:     seriesName,
		ReportFileName: reportFilename,
		FullName:       fmt.Sprintf("%s/%s", lbrDirectory, reportFilename),
	}
}

func (lr *LabradarFile) removeNullCharacters(newDirectory string) (string, error) {
	// Not sure if it's better to overwrite the existing file or create a new one.
	originalBytes, err := ioutil.ReadFile(lr.FullName)
	if err != nil {
		return "", err
	}

	if !bytes.ContainsAny(originalBytes, "\x00") {
		return "", nil
	}

	newFile := path.Join(newDirectory, fmt.Sprintf("%s.csv", lr.SeriesName))
	fileContentsWithoutNull := bytes.ReplaceAll(originalBytes, []byte{'\x00'}, []byte{})
	err = ioutil.WriteFile(newFile, fileContentsWithoutNull, 0644)
	if err == nil {
		return newFile, nil
	} else {
		return newFile, err
	}
}

func (lr *LabradarFile) exists(lbrDirectory string) bool {
	s := filepath.Join(lbrDirectory, lr.ReportFileName)
	_, err := os.Stat(s)
	return err == nil
}
