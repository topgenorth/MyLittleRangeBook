// Package build holds all the code that is specific to compiling the app.
package build

import (
	"path/filepath"
	"runtime"
)

type Artifacts struct {
	CurrentDir     string
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
		CurrentDir:     dir,
		ExecutableName: exec,
		MainGo:         filepath.Join(dir, "cmd", "mlrb", "main.go"),
		CompiledApp:    filepath.Join(dir, "output", exec),
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
