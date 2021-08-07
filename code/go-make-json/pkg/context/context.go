package context

import (
	"github.com/spf13/afero"
	"io"
	"os"
)

type Context struct {
	Afero afero.Afero
	In    io.Reader
	Out   io.Writer
	Err   io.Writer
}

func New() *Context {
	return &Context{
		Afero: afero.Afero{
			Fs: afero.NewOsFs(),
		},
		In:  os.Stdin,
		Out: os.Stdout,
		Err: os.Stderr,
	}
}
