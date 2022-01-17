package labradar

import (
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar/fs"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"path/filepath"
)

type JsonSeriesWriter1 struct {
	*config.Config
	FileSystem *afero.Afero
}

func (w *JsonSeriesWriter1) Write(s OldSeries) error {

	dir, err := w.GetHomeDir()
	if err != nil {
		return err
	}

	outputFileName := filepath.Join(dir, s.Labradar.SeriesName+".json")
	if !fs.DeleteFile(outputFileName, w.Config) {
		return fmt.Errorf("cannot write to the file %s: %v", outputFileName, err)
	}

	err = w.FileSystem.WriteFile(outputFileName, s.ToJsonBytes(), 0644)
	if err != nil {
		return series.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Could not write to the file %s. %v", outputFileName, err),
		}
	}

	return nil
}
