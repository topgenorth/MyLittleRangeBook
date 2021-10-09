package labradar

import (
	"fmt"
	"opgenorth.net/labradar/pkg/config"
	"path"
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
