// Package build holds all the code that is specific to compiling the app.
package build

import (
	"path/filepath"
	"runtime"
)

type Artifacts struct {
	CompiledApp    string
	ExecutableName string
	OutputDir      string
	MainGo         string
	InstalledApp   string
}

func New(dir string) Artifacts {

	exec := executableName()
	id := filepath.Join(dir, "..", "..", "bin")

	a := Artifacts{
		ExecutableName: exec,
		CompiledApp:    filepath.Join(dir, "output", exec),
		MainGo:         filepath.Join(dir, "cmd", "mlrb", "main.go"),
		InstalledApp:   filepath.Join(id, exec),
	}
	return a
}

func executableName() string {
	var filename string
	if runtime.GOOS == "windows" {
		filename = "mlrb.exe"
	} else {
		filename = "mlrb"
	}

	return filename
}
