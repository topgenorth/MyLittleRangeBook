package labradar

import (
	"github.com/spf13/afero"
)

var (
	afs *afero.Afero
)

func init() {
	fs := afero.NewOsFs()
	afs = &afero.Afero{Fs: fs}
}
