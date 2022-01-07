package io

import (
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
)

type JsonSeriesWriter1 struct {
	*config.Config
	FileSystem *afero.Afero
}

func (w *JsonSeriesWriter1) Write(s labradar.Series) error {
	name, err := filenameForSeries(s, w.Config, "json")
	if err != nil {
		return err
	}

	err = w.FileSystem.WriteFile(name, s.ToJsonBytes(), 0644)
	if err != nil {
		return labradar.SeriesError{
			Number: s.Number,
			Msg:    fmt.Sprintf("Could not write to the file %s. %v", name, err),
		}
	}

	return nil
}
