package labradar

import (
	"bytes"
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"os"
	"text/template"
)

type StdOutSeriesWriter1 struct {
	TemplateString string
}

func (w *StdOutSeriesWriter1) Write(s OldSeries) error {
	t, err := template.New("OldSeriesWriter").Parse(w.TemplateString)
	if err != nil {
		return series.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error creating text.template: %v", err),
		}
	}

	err = t.Execute(os.Stdout, s)
	if err != nil {
		return series.SeriesError{
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

func (w *ReadMeSeriesWriter) Write(s OldSeries) error {
	var tmpl string
	if w.OldFormat {
		tmpl = TMPL_README_LINE_OLD_FORMAT
	} else {
		tmpl = TMPL_README_LINE
	}

	t, err := template.New("ToStringSerialWriter").Parse(tmpl)
	if err != nil {
		return series.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error creating text.template: %v", err),
		}
	}

	var line bytes.Buffer
	err = t.Execute(&line, s)
	if err != nil {
		return series.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error writing template: %v", err),
		}
	}

	w.Output = line.String()

	return nil
}

const TMPL_README_LINE = `| {{.Number}} | {{.LoadData}} | {{.Firearm.Name}} | {{.Notes}} | {{.Labradar.Date}} |`
const TMPL_README_LINE_OLD_FORMAT = `| {{.Number}} | {{.LoadData}};{{.Notes}} | {{.Firearm.Name}} | {{.Labradar.Date}} |`
const TMPL_SUMMARIZE_SERIES = `----
Labradar OldSeries {{.Labradar.SeriesName}}

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

| OldSeries Number | Ammo | Firearm | Notes | Date |
| :---:         | :--- | :-----  | :--- | :---:
| {{.Number}} | {{.LoadData}} | {{.Firearm.Name}} | {{.Notes}} | {{.Labradar.Date}} |
`
