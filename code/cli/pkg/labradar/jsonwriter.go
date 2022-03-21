package labradar

import (
	"encoding/json"
	"fmt"
	"github.com/spf13/afero"
	"path/filepath"
)

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
