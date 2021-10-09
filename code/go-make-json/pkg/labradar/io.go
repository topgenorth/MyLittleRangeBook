package labradar

import (
	"fmt"
	"github.com/spf13/afero"
	"os"
	"path/filepath"
)

func closeFile(f afero.File) {
	err := f.Close()
	if err != nil {
		fmt.Println(err)
	}
}

func openFile(filename string, a afero.Afero) (afero.File, error) {
	file, err := a.Open(filename)
	if err != nil {
		return nil, err
	}

	return file, nil
}

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
