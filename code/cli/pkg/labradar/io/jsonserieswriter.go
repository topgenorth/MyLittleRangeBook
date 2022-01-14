package io

import (
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"path/filepath"
)

type JsonSeriesWriter1 struct {
	*config.Config
	FileSystem *afero.Afero
}

func (w *JsonSeriesWriter1) Write(s labradar.Series) error {

	dir, err := w.GetHomeDir()
	if err != nil {
		return err
	}

	outputFileName := filepath.Join(dir, s.Labradar.SeriesName+".json")
	if !DeleteFile(outputFileName, w.Config) {
		return fmt.Errorf("cannot write to the file %s: %v", outputFileName, err)
	}

	err = w.FileSystem.WriteFile(outputFileName, s.ToJsonBytes(), 0644)
	if err != nil {
		return labradar.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Could not write to the file %s. %v", outputFileName, err),
		}
	}

	return nil
}
