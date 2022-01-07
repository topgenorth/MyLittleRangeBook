package labradar

import (
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"os"
	"path/filepath"
	"text/template"
)

type SeriesWriter struct {
	C *config.Config
}

func (sw *SeriesWriter) WriteStdOut(s Series, templateString string) error {
	t, err := template.New("SeriesWriter").Parse(templateString)
	if err != nil {
		return SeriesError{
			number: s.Number,
			msg:    fmt.Sprintf("Error creating text.template: %v", err),
		}
	}

	err = t.Execute(os.Stdout, s)
	if err != nil {
		return SeriesError{
			number: s.Number,
			msg:    fmt.Sprintf("Error writing template: %v", err),
		}
	}
	return nil
}

func (sw *SeriesWriter) WriteJson(s Series) error {
	name, err := filenameForSeries(s, sw.C, "json")
	if err != nil {
		return err
	}

	err = sw.C.FileSystem.WriteFile(name, s.ToJsonBytes(), 0644)
	if err != nil {
		return SeriesError{
			number: s.Number,
			msg:    fmt.Sprintf("Could not write to the file %s. %v", name, err),
		}
	}

	return nil
}

func filenameForSeries(s Series, c *config.Config, ext string) (string, error) {
	dir, err := c.GetHomeDir()
	if err != nil {
		return "", err
	}
	outputFileName := filepath.Join(dir, s.Labradar.SeriesName+"."+ext)
	if !DeleteFile(outputFileName, c) {
		return "", SeriesError{number: s.Number, msg: fmt.Sprintf("The file %s exists.", outputFileName)}
	}

	return "", nil
}
