package mycontext

import (
	"github.com/spf13/afero"
	"io"
	"os"
)

type MyContext struct {
	Afero afero.Afero
	In    io.Reader
	Out   io.Writer
	Err   io.Writer
}

func New() *MyContext {
	return &MyContext{
		Afero: afero.Afero{
			Fs: afero.NewOsFs(),
		},
		In:  os.Stdin,
		Out: os.Stdout,
		Err: os.Stderr,
	}
}
