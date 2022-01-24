package jsonwriter

import (
	"encoding/json"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
)

// JsonFileNameProvider is a function that will return the name of the JSON file for a series.
type JsonFileNameProvider = func() string
type JsonWriter struct {
	afs      afero.Fs
	filename JsonFileNameProvider
}

func New(aferoFs afero.Fs, nameProvider JsonFileNameProvider) JsonWriter {
	return JsonWriter{
		afs:      aferoFs,
		filename: nameProvider,
	}
}

func (w *JsonWriter) Write(s series.LabradarSeries) error {
	bytes, err := json.MarshalIndent(s, "", "    ")
	if err != nil {
		return err
	}

	if err = afero.WriteFile(w.afs, w.filename(), bytes, 0644); err != nil {
		return err
	}

	return nil
}
