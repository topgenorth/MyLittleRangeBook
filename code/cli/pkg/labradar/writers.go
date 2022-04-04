package labradar

import (
	"bytes"
	"encoding/json"
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"io"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"path/filepath"
	"text/template"
)

// SeriesWriter is an interface to persisting a OldSeries to something a person might read (HTML, JSON, Markdown, etc).
type SeriesWriter interface {
	// Write is used to persist a series.Series value to something that a person might read.
	Write(s Series) error
}

// JsonSeriesWriter will persist a given Series to a JSON file.
type JsonSeriesWriter struct {
	*config.Config
}

// Write will overwrite any existing file.
func (w *JsonSeriesWriter) Write(s *Series, fn DirectoryProviderFn) error {

	outputFileName := filepath.Join(fn(), s.Number.String()+".json")

	if err := w.Filesystem.Remove(outputFileName); err != nil {
		return err
	}

	data, err := json.Marshal(&s)
	if err != nil {
		return err
	}

	if err := w.Filesystem.WriteFile(outputFileName, data, 0644); err != nil {
		return err
	}

	logrus.Tracef("Saved %s to the file %s.", s.Number.String(), outputFileName)
	return nil
}

type ReadMeSeriesWriter struct {
	Output    string
	OldFormat bool
}

func (w *ReadMeSeriesWriter) Write(s Series) error {
	var tmpl string
	if w.OldFormat {
		tmpl = TMPL_README_LINE_OLD_FORMAT
	} else {
		tmpl = TMPL_README_LINE
	}

	t, err := template.New("ToStringSerialWriter").Parse(tmpl)
	if err != nil {
		return Error{
			Number: s.Number.Int(),
			Msg:    fmt.Sprintf("Error creating text.template: %v", err),
		}
	}

	var line bytes.Buffer
	err = t.Execute(&line, s)
	if err != nil {
		return Error{
			Number: s.Number.Int(),
			Msg:    fmt.Sprintf("Error writing template: %v", err),
		}
	}

	w.Output = line.String()

	return nil
}

const TMPL_README_LINE = `| {{.Number}} | {{.LoadData}} | {{.Firearm.Name}} | {{.Notes}} | {{.Labradar.Date}} |`
const TMPL_README_LINE_OLD_FORMAT = `| {{.Number}} | {{.LoadData}};{{.Notes}} | {{.Firearm.Name}} | {{.Labradar.Date}} |`

const TMPL_DESCRIBE_SERIES = `
# Description of Labradar series

For ammo, stick with the format:
    Cartridge; Bullet; Powder; COAL or CBTO

| OldSeries Number | Ammo | Firearm | Notes | Date |
| :---:         | :--- | :-----  | :--- | :---:
| {{.Number}} | {{.LoadData}} | {{.Firearm.Name}} | {{.Notes}} | {{.Labradar.Date}} |
`

func NewJsonWriter(aferoFs afero.Fs, nameProvider JsonFileNameProvider) JsonWriter {
	return JsonWriter{
		afs:      aferoFs,
		filename: nameProvider,
	}
}

func DefaultJsonFileProvider(dir string, seriesNumber int) JsonFileNameProvider {
	seriesname := fmt.Sprintf("SR%04d", seriesNumber)
	filename := filepath.Join(dir, seriesname, seriesname+".json")

	return func() string {
		return filename
	}

}

func (w *JsonWriter) Write(s Series) error {
	bytes, err := json.MarshalIndent(s, "", "    ")
	if err != nil {
		return err
	}

	if err = afero.WriteFile(w.afs, w.filename(), bytes, 0644); err != nil {
		return err
	}

	return nil
}

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
DeviceDirectory Id       : {{.DeviceId}}
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
