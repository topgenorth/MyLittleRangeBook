package labradar

import (
	"fmt"
	"io"
	"text/template"
)

type SeriesTemplateType string

const (
	SimplePlainText      SeriesTemplateType = "SimplePlainText"
	DescriptivePlainText SeriesTemplateType = "DescriptivePlainText"
	JSON                 SeriesTemplateType = "Json"
)

// SummaryWriter is used to display the summary of a given series.
type SummaryWriter struct {
	Out      io.Writer
	Template SeriesTemplateType
}

func GetSummaryWriter(out io.Writer, template SeriesTemplateType) SummaryWriter {
	return SummaryWriter{
		Out:      out,
		Template: template,
	}
}

func parseTemplateType(t SeriesTemplateType) string {
	if t == SimplePlainText {
		return tmplSimplePlainText
	}

	if t == DescriptivePlainText {
		return tmplDescriptivePlainText
	}

	return ""
}
func (w *SummaryWriter) Write(s Series) error {
	if w.Template == JSON {
		return fmt.Errorf("cannot write the series; unknown type %s", w.Template)
	}

	t, err := template.New("SeriesSummary").Parse(parseTemplateType(w.Template))
	if err != nil {
		return Error{
			Number: s.Number.Int(),
			Msg:    fmt.Sprintf("could not load the template %s", w.Template),
		}
	}

	err = t.Execute(w.Out, s)
	if err != nil {
		return fmt.Errorf("failed to execute the template  %s for %s: %w", w.Template, s.String(), err)
	}
	return nil
}

const tmplSimplePlainText = `
----
Labradar        : {{.DeviceId}}
Labradar Series : {{.Number}}
Date            : {{.Date}} {{.Time}}

Number of Shots : {{.CountOfShots}}
Avg Velocity    : {{.Velocities.Average}}{{.UnitsOfMeasure.Velocity}}
Standard Dev    : {{.Velocities.StdDev}}{{.UnitsOfMeasure.Velocity}}
Extreme Spread  : {{.Velocities.ExtremeSpread}}{{.UnitsOfMeasure.Velocity}}
----

`

const tmplDescriptivePlainText = `
---
Device Id       : {{.DeviceId}}
Date            : {{.Date}} {{.Time}}

Labradar Series : {{.Number}}
Number of Shots : {{.CountOfShots}}

Firearm         : {{.Firearm}}
Load            : {{.LoadData.Projectile }}, {{.LoadData.Powder}}
Notes:          : {{.Notes}}

Avg Velocity    : {{.Velocities.Average}}{{.UnitsOfMeasure.Velocity}}
Standard Dev    : {{.Velocities.StdDev}}{{.UnitsOfMeasure.Velocity}}
Extreme Spread  : {{.Velocities.ExtremeSpread}}{{.UnitsOfMeasure.Velocity}}
{{ range $i, $v:= .Velocities.Values}}    Shot {{$i}}: {{$v}} 
{{ end }}
---

`
