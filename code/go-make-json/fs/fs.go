package fs

import (
	"bufio"
	"fmt"
	"github.com/spf13/afero"
	jww "github.com/spf13/jwalterweatherman"
	"opgenorth.net/labradar/labradar"
	"opgenorth.net/labradar/util"
)


func LoadLabradardCsv(ls *labradar.LabradarSeries) error {

	a := afero.Afero{
		Fs: afero.NewOsFs(),
	}
	file, err := a.Open(util.GetPathToLabradarSeries(ls.Number))
	defer func(f afero.File) {
		err := f.Close()
		if err != nil {
			jww.ERROR.Println(err)
		}
	}(file)
	if err != nil {
		return err
	}

	s := bufio.NewScanner(file)
	var i = 0
	for s.Scan() {
		line := util.FixupLabradarLine(s.Text())

		fmt.Printf("%d: %s", i, line)
		fmt.Println()
		i++
	}

	if err := s.Err(); err != nil {
		return err
	}

	return nil
}
