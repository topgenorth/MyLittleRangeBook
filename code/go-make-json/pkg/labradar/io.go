package labradar

import (
	"github.com/spf13/afero"
	"os"
	"path/filepath"
)



func SaveLabradarSeriesToJson(ls *Series, cfg *ReadCsvConfig) error {
	outputFileName := filepath.Join(cfg.OutputDir, ls.Labradar.SeriesName+".json")

	err := deleteFileIfExists(cfg.Context.Filesystem, outputFileName)
	if err != nil {
		return err
	}

	err2 := cfg.Context.Filesystem.WriteFile(outputFileName, ls.ToJson(), 0644)
	if err2 != nil {
		return err2
	}

	return nil
}


func deleteFileIfExists(a afero.Afero, fileName string) error {
	exists, _ := a.Exists(fileName)
	if exists {
		err := os.Remove(fileName)
		if err != nil {
			return err
		}
	}
	return nil
}
