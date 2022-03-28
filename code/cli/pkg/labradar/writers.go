package labradar

import (
	"bytes"
	"encoding/json"
	"fmt"
	"github.com/sirupsen/logrus"
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
