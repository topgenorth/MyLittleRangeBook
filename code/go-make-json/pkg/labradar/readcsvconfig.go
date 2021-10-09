package labradar

import (
	"fmt"
	"log"
	"opgenorth.net/labradar/pkg/config"
	"os"
	"path"
	"path/filepath"
	"strings"
)

type ReadCsvConfig struct {
	*config.Config
	SeriesNumber int
	InputDir     string
	OutputDir    string
}

func (c *ReadCsvConfig) GetInputFilename() string {
	stub := fmt.Sprintf("%04d", c.SeriesNumber)
	filename := fmt.Sprintf("%s Report.csv", stub)
	return path.Join(c.InputDir, stub, filename)
}

func (c *ReadCsvConfig) GetOutputFilename() string {
	stub := fmt.Sprintf("%04d", c.SeriesNumber)
	filename := fmt.Sprintf("%s.json", stub)
	return path.Join(c.OutputDir, filename)
}

func (c *ReadCsvConfig) GetLabradarFilesInDir() []string {
	var filenames []string

	err := filepath.Walk(c.InputDir, func(path string, info os.FileInfo, err error) error {
		if !info.IsDir() && isLabradarCsvFile(path) {
			filenames = append(filenames, path)
		}
		return nil
	})
	if err != nil {
		log.Fatal(err)
	}

	return filenames
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
