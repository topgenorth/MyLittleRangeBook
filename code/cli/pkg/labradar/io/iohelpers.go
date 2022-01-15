package io

import (
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"os"
	"path"
)

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

func CloseFile(f afero.File) {
	err := f.Close()
	if err != nil {
		fmt.Println(err)
	}
}

// FilenameForSeries Given the Number of a series and the root directory of the Labradar files, infer the filename of the Labradar
// CSV file that holds the results of the series.
func FilenameForSeries(labradarRootDirectory string, seriesNumber int) string {
	stub := fmt.Sprintf("%04d", seriesNumber)
	//goland:noinspection SpellCheckingInspection
	subdir := fmt.Sprintf("SR%s", stub)
	filename := fmt.Sprintf("SR%s Report.csv", stub)
	p := path.Join(labradarRootDirectory, subdir, filename)
	return p
}
