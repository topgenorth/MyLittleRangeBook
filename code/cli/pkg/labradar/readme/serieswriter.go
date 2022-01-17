package readme

import (
	"bytes"
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"text/template"
)

const TMPL_README_SERIES_LINE = `| {{.Number}} | {{.LoadData}};{{.Notes}} | {{.Firearm.Name}} | {{.Labradar.Date}} |`

type StringWriter struct {
	Value string
}

func (w *StringWriter) For(r *ReadmeMd) *StringWriter {
	return w
}

func (w StringWriter) String() string {
	return w.Value
}
func (w *StringWriter) Write(s labradar.OldSeries) error {
	w.Value = ""

	t, err := template.New("labradarReadmeStringWriter").Parse(TMPL_README_SERIES_LINE)
	if err != nil {
		return series.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error creating text.template: %v", err),
		}
	}

	var tpl bytes.Buffer
	if err := t.Execute(&tpl, s); err != nil {
		return series.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error writing template: %v", err),
		}
	}

	w.Value = tpl.String()
	return nil
}
