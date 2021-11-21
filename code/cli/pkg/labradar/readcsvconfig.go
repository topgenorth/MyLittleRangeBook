package labradar

import (
	"fmt"
	"log"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"os"
	"path"
	"path/filepath"
	"strings"
)

type LabradarCsvFile struct {
	*config.Config
	SeriesNumber int
	InputDir     string
	OutputDir    string
}

func (c *LabradarCsvFile) GetInputFilename() string {
	stub := fmt.Sprintf("%04d", c.SeriesNumber)
	subdir := fmt.Sprintf("SR%s", stub)
	filename := fmt.Sprintf("SR%s Report.csv", stub)
	p:= path.Join(c.InputDir, subdir, filename)
	return p
}

func (c *LabradarCsvFile) GetOutputFilename() string {
	stub := fmt.Sprintf("%04d", c.SeriesNumber)
	filename := fmt.Sprintf("%s.json", stub)
	return path.Join(c.OutputDir, filename)
}

func (c *LabradarCsvFile) ListFilesInDir() []string {
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
