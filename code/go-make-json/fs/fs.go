package fs

import (
	"bufio"
	"github.com/spf13/afero"
	jww "github.com/spf13/jwalterweatherman"
	"opgenorth.net/labradar/labradar"
	"opgenorth.net/labradar/util"
)

func LoadLabradarSeriesFromCsv(seriesNumber int, ls *labradar.LabradarSeries) error {
	a := afero.Afero{
		Fs: afero.NewOsFs(),
	}
	file, err := a.Open(util.GetPathToLabradarSeries(seriesNumber))
	defer func(f afero.File) {
		err := f.Close()
		if err != nil {
			jww.ERROR.Println(err)
		}
	}(file)
	if err != nil {
		return err
	}

	skanner := bufio.NewScanner(file)
	var lineNumber = 0
	for skanner.Scan() {
		lineOfData := labradar.CreateLine(lineNumber, skanner.Text())
		ls.ParseLine(lineOfData)
		lineNumber++
	}

	if err := skanner.Err(); err != nil {
		return err
	}

	return nil
}
