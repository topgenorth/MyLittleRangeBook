package labradar

import (
	"bytes"
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"text/template"
)

// SeriesWriter is an interface to persisting a OldSeries to something a person might read (HTML, JSON, Markdown, etc).
type SeriesWriter interface {
	// Write is used to persist a series.LabradarSeries value to something that a person might read.
	Write(s series.LabradarSeries) error
}

// JsonSeriesWriter will persist a given LabradarSeries to a JSON file.
type JsonSeriesWriter struct {
	*config.Config
	FileSystem *afero.Afero
}

func (w *JsonSeriesWriter) Write(s series.LabradarSeries) error {

	logrus.Warn("Not implemented.")
	//dir, err := w.GetHomeDir()
	//if err != nil {
	//	return err
	//}
	//
	//outputFileName := filepath.Join(dir, s.Labradar.SeriesName+".json")
	//if !fs.DeleteFile(outputFileName, w.Config) {
	//	return fmt.Errorf("cannot write to the file %s: %v", outputFileName, err)
	//}
	//
	//err = w.FileSystem.WriteFile(outputFileName, s.ToJsonBytes(), 0644)
	//if err != nil {
	//	return series.SeriesError{
	//		Number: s.Number,
	//		Msg:    fmt.Sprintf("Could not write to the file %s. %v", outputFileName, err),
	//	}
	//}

	return nil
}

type ReadMeSeriesWriter struct {
	Output    string
	OldFormat bool
}

func (w *ReadMeSeriesWriter) Write(s series.LabradarSeries) error {
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

const TMPL_DESCRIBE_SERIES = `
# Description of Labradar series

For ammo, stick with the format:
    Cartridge; Bullet; Powder; COAL or CBTO

| OldSeries Number | Ammo | Firearm | Notes | Date |
| :---:         | :--- | :-----  | :--- | :---:
| {{.Number}} | {{.LoadData}} | {{.Firearm.Name}} | {{.Notes}} | {{.Labradar.Date}} |
`
