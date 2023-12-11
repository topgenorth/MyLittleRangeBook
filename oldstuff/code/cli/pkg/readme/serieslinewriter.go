package readme

import (
	"bytes"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"text/template"
)

// SeriesLineWriter will render the series.Labradar string to a single line of text for the Readme file.
type SeriesLineWriter struct {
	Output    string
	OldFormat bool
}

func (w *SeriesLineWriter) Write(s labradar.Series) error {
	var tmpl string
	if w.OldFormat {
		tmpl = tmplReadmeLineV1
	} else {
		tmpl = tmplReadmeLineV2
	}

	t, err := template.New("ToStringSerialWriter").Parse(tmpl)
	if err != nil {
		return err
	}

	var line bytes.Buffer
	err = t.Execute(&line, s)
	if err != nil {
		return err
	}

	w.Output = line.String()

	return nil
}

// [TO20220125] Original format
const tmplReadmeLineV1 = `| {{.SeriesName}} | {{.LoadData}};{{.Notes}} | {{.Firearm.Name}} | {{.Date}} |`

// [TO20220125] Switched to this format in 2022
const tmplReadmeLineV2 = `| {{.SeriesName}} | {{.LoadData}} | {{.Firearm.Name}} | {{.Notes}} | {{.Date}} |`
