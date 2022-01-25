package labradar

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
)

// SeriesWriter is an interface to persisting a OldSeries to something a person might read (HTML, JSON, Markdown, etc).
type SeriesWriter interface {
	// Write is used to persist a series.LabradarSeries value to something that a person might read.
	Write(s series.LabradarSeries) error
}

// JsonSeriesWriter will persist a given LabradarSeries to a JSON file.
type JsonSeriesWriter struct {
	*config.Config
	FileSystem *afero.Afero
}

func (w *JsonSeriesWriter) Write(s series.LabradarSeries) error {

	logrus.Warn("Not implemented.")
	//dir, err := w.GetHomeDir()
	//if err != nil {
	//	return err
	//}
	//
	//outputFileName := filepath.Join(dir, s.Labradar.SeriesName+".json")
	//if !fs.DeleteFile(outputFileName, w.Config) {
	//	return fmt.Errorf("cannot write to the file %s: %v", outputFileName, err)
	//}
	//
	//err = w.FileSystem.WriteFile(outputFileName, s.ToJsonBytes(), 0644)
	//if err != nil {
	//	return series.SeriesError{
	//		Number: s.Number,
	//		Msg:    fmt.Sprintf("Could not write to the file %s. %v", outputFileName, err),
	//	}
	//}

	return nil
}

const TMPL_DESCRIBE_SERIES = `
# Description of Labradar series

For ammo, stick with the format:
    Cartridge; Bullet; Powder; COAL or CBTO

| OldSeries Number | Ammo | Firearm | Notes | Date |
| :---:         | :--- | :-----  | :--- | :---:
| {{.Number}} | {{.LoadData}} | {{.Firearm.Name}} | {{.Notes}} | {{.Labradar.Date}} |
`
