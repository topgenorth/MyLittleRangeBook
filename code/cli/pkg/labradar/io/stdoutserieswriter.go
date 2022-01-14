package io

import (
	"bytes"
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"os"
	"text/template"
)

type StdOutSeriesWriter1 struct {
	TemplateString string
}

const TMPL_DESCRIBE_SERIES = `
# Description of Labradar series

For ammo, stick with the format:
    Cartridge; Bullet; Powder; COAL or CBTO

| Series Number | Ammo | Firearm | Notes | Date |
| :---:         | :--- | :-----  | :--- | :---:
| {{.Number}} | {{.LoadData}} | {{.Firearm.Name}} | {{.Notes}} | {{.Labradar.Date}} |
`

func (w *StdOutSeriesWriter1) Write(s labradar.Series) error {
	t, err := template.New("OldSeriesWriter").Parse(w.TemplateString)
	if err != nil {
		return labradar.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error creating text.template: %v", err),
		}
	}

	err = t.Execute(os.Stdout, s)
	if err != nil {
		return labradar.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error writing template: %v", err),
		}
	}
	return nil
}

type ReadMeSeriesWriter struct {
	Output    string
	OldFormat bool
}

const TMPL_README_LINE = `| {{.Number}} | {{.LoadData}} | {{.Firearm.Name}} | {{.Notes}} | {{.Labradar.Date}} |`
const TMPL_README_LINE_OLD_FORMAT = `| {{.Number}} | {{.LoadData}};{{.Notes}} | {{.Firearm.Name}} | {{.Labradar.Date}} |`

func (w *ReadMeSeriesWriter) Write(s labradar.Series) error {
	var tmpl string
	if w.OldFormat {
		tmpl = TMPL_README_LINE_OLD_FORMAT
	} else {
		tmpl = TMPL_README_LINE
	}

	t, err := template.New("ToStringSerialWriter").Parse(tmpl)
	if err != nil {
		return labradar.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error creating text.template: %v", err),
		}
	}

	var line bytes.Buffer
	err = t.Execute(&line, s)
	if err != nil {
		return labradar.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error writing template: %v", err),
		}
	}

	w.Output = line.String()

	return nil
}

const TMPL_SUMMARIZE_SERIES = `----
Labradar Series {{.Labradar.SeriesName}}

Number of Shots: {{.TotalNumberOfShots}}
Average Velocity: {{.Velocities.Average}}{{.Labradar.Units.Velocity}}
Standard Deviation: {{.Velocities.StandardDeviation}}{{.Labradar.Units.Velocity}}
Extreme Spread: {{.Velocities.ExtremeSpread}}{{.Labradar.Units.Velocity}}
----
`
