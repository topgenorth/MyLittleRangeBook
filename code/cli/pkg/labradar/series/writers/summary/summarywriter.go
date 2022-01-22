package summary

import (
	"fmt"
	"io"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"text/template"
)

type SummaryTemplateType string

const (
	PlainText SummaryTemplateType = "PlainText"
	JSON      SummaryTemplateType = "Json"
)

// SummaryWriter is used to display the summary of a given series.
type SummaryWriter struct {
	Out  io.Writer
	Type SummaryTemplateType
}

func (w *SummaryWriter) Write(s series.LabradarSeries) error {

	if w.Type != PlainText {
		return fmt.Errorf("cannot write the series; unknown type %s", w.Type)
	}

	t, err := template.New("SeriesSummary").Parse(tmplPlainText)
	if err != nil {
		return series.SeriesError{
			Number: s.Number,
			Msg:    "Could not load the template.",
		}
	}

	err = t.Execute(w.Out, s)
	if err != nil {
		return series.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Could not render the template."),
		}
	}
	return nil
}

const tmplPlainText = `
----
Labradar Series : SR{{.SeriesName}}
Date            : {{.Date}} {{.Time}}

Number of Shots : {{.TotalNumberOfShots}}
Avg Velocity    : {{.Velocities.Average}}{{.UnitsOfMeasure.Velocity}}
Standard Dev    : {{.Velocities.StdDev}}{{.UnitsOfMeasure.Velocity}}
Extreme Spread  : {{.Velocities.ExtremeSpread}}{{.UnitsOfMeasure.Velocity}}
----

`
