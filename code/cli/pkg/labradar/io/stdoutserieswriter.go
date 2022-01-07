package io

import (
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"os"
	"text/template"
)

type StdOutSeriesWriter1 struct {
	TemplateString string
}

func (w *StdOutSeriesWriter1) Write(s labradar.Series) error {
	t, err := template.New("OldSeriesWriter").Parse(w.TemplateString)
	if err != nil {
		return labradar.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error creating text.template: %v", err),
		}
	}

	err = t.Execute(os.Stdout, s)
	if err != nil {
		return labradar.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Error writing template: %v", err),
		}
	}
	return nil
}
