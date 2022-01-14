package io

import (
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"os"
	"path/filepath"
)

func filenameForSeries(s labradar.Series, c *config.Config, ext string) (string, error) {
	dir, err := c.GetHomeDir()
	if err != nil {
		return "", err
	}
	outputFileName := filepath.Join(dir, s.Labradar.SeriesName+"."+ext)
	if !DeleteFile(outputFileName, c) {
		return "", labradar.SeriesError{Number: s.Number, Msg: fmt.Sprintf("The file %s exists.", outputFileName)}
	}
	return "", nil
}
func DeleteFile(s string, c *config.Config) bool {
	exists, err := c.FileSystem.Exists(s)
	if err != nil {
		return false
	}
	if exists {
		err := os.Remove(s)
		// TODO [TO20220105] Log a warning.
		if err != nil {
			return false
		}
	}
	return true
}
