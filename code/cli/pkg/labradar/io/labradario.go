package io

import (
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"path/filepath"
)

func filenameForSeries(s labradar.Series, c *config.Config, ext string) (string, error) {
	dir, err := c.GetHomeDir()
	if err != nil {
		return "", err
	}
	outputFileName := filepath.Join(dir, s.Labradar.SeriesName+"."+ext)
	if !labradar.DeleteFile(outputFileName, c) {
		return "", labradar.SeriesError{Number: s.Number, Msg: fmt.Sprintf("The file %s exists.", outputFileName)}
	}

	return "", nil
}

const TMPL_SUMMARIZE_SERIES = `----
Labradar Series {{.Labradar.SeriesName}}

Number of Shots: {{.TotalNumberOfShots}}
Average Velocity: {{.Velocities.Average}}{{.Labradar.Units.Velocity}}
Standard Deviation: {{.Velocities.StandardDeviation}}{{.Labradar.Units.Velocity}}
Extreme Spread: {{.Velocities.ExtremeSpread}}{{.Labradar.Units.Velocity}}
----
`

const TMPL_DESCRIBE_SERIES = `
# Description of Labradar series

For ammo, stick with the format:
    Cartridge; Bullet; Powder; COAL or CBTO

| Series Number | Ammo | Firearm | Notes | Date |
| :---:         | :--- | :-----  | :--- | :---:
| {{.Number}} | {{.LoadData}} | {{.Firearm.Name}} | {{.Notes}} | {{.Labradar.Date}} |
`

type SeriesWriter interface {
	Write(s labradar.Series) error
}
