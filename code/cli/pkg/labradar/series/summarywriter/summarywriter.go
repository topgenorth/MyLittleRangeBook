package summarywriter

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"io"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
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

func New(out io.Writer, template SeriesTemplateType) SummaryWriter {
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
func (w *SummaryWriter) Write(s series.LabradarSeries) error {

	if w.Template == JSON {
		return fmt.Errorf("cannot write the series; unknown type %s", w.Template)
	}

	t, err := template.New("SeriesSummary").Parse(parseTemplateType(w.Template))
	if err != nil {
		return series.Error{
			Number: s.Number,
			Msg:    fmt.Sprintf("could not load the template %s", w.Template),
		}
	}

	err = t.Execute(w.Out, s)
	if err != nil {
		m := fmt.Sprintf("could not render the template %s", w.Template)
		logrus.WithError(err).Errorf(m)
		return series.Error{
			Number: s.Number,
			Msg:    m,
		}
	}
	return nil
}

const tmplSimplePlainText = `
----
Labradar Series : {{.SeriesName}}
Date            : {{.Date}} {{.Time}}

Number of Shots : {{.TotalNumberOfShots}}
Avg Velocity    : {{.Velocities.Average}}{{.UnitsOfMeasure.Velocity}}
Standard Dev    : {{.Velocities.StdDev}}{{.UnitsOfMeasure.Velocity}}
Extreme Spread  : {{.Velocities.ExtremeSpread}}{{.UnitsOfMeasure.Velocity}}
----

`

const tmplDescriptivePlainText = `
---
Labradar Series : {{.SeriesName}}
Firearm         : {{.Firearm}}
Load            : {{.LoadData.Projectile }}, {{.LoadData.Powder}}
Notes:          : {{.Notes}}

Device Id       : {{.DeviceId}}
Date            : {{.Date}} {{.Time}}
Number of Shots : {{.TotalNumberOfShots}}

VelocityData (in {{.UnitsOfMeasure.Velocity}})
Avg Velocity    : {{.Velocities.Average}}{{.UnitsOfMeasure.Velocity}}
Standard Dev    : {{.Velocities.StdDev}}{{.UnitsOfMeasure.Velocity}}
Extreme Spread  : {{.Velocities.ExtremeSpread}}{{.UnitsOfMeasure.Velocity}}
{{ range $i, $v:= .Velocities.Values}} * Shot {{$i}}: {{$v}}
{{ end }}
---

`
