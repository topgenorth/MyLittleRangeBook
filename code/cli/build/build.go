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

var mlrbDotGo = []string{"apps", "mlrb", "mlrb.go"}

func New(dir string) Artifacts {
	exec := executableName()

	installedAppPath := append([]string{dir}, "..", "..", "bin", exec)
	intermediatePath := append([]string{dir}, "output", exec)
	mlrbDotGoPath := append([]string{dir}, mlrbDotGo...)

	a := Artifacts{
		CurrentDir:     dir,
		ExecutableName: exec,
		MainGo:         filepath.Join(mlrbDotGoPath...),
		CompiledApp:    filepath.Join(intermediatePath...),
		InstalledApp:   filepath.Join(installedAppPath...),
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
