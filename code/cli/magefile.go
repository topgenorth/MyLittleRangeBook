//go:build mage
// +build mage

package main

import (
	"fmt"
	"github.com/magefile/mage/mg"
	"github.com/sirupsen/logrus"
	"opgenorth.net/mylittlerangebook/build"
	"opgenorth.net/mylittlerangebook/fs"
	"os"
	"os/exec"
	"path/filepath"
)

// A build step that requires additional params, or platform specific steps for example
func Build() error {
	b := buildArtifacts()

	mg.Deps(InstallDeps)
	fmt.Printf("Compiling %s...\n", b.CompiledApp)
	cmd := exec.Command("go", "build", "-o", b.CompiledApp, b.MainGo)
	return cmd.Run()
}

func buildArtifacts() build.Artifacts {
	currentDir, err := fs.CurrentWd()
	if err != nil {
		logrus.WithError(err).Panicf("Problem figuring out our build environment.")
	}
	b := build.New(currentDir)
	return b
}

// A custom install step if you need your bin someplace other than go/bin
func Install() error {
	b := buildArtifacts()

	mg.Deps(Build)

	d := filepath.Dir(b.InstalledApp)
	if _, err := os.Stat(d); os.IsNotExist(err) {
		if e2 := os.Mkdir(d, 0755); e2 != nil {
			logrus.WithError(e2).Panicf("Could not create the install directory %s!", d)
		}
	}

	if _, err := os.Stat(d); !os.IsNotExist(err) {
		_ = os.Remove(b.InstalledApp)
	}

	fmt.Printf("Installing to %s\n", b.InstalledApp)
	return fs.CopyFile(b.CompiledApp, b.InstalledApp)
}

// Manage your deps, or running package managers.
func InstallDeps() error {
	fmt.Println("This doesn't actually do anything right now.")
	//b := buildArtifacts()

	//cmd := exec.Command("go", "get", "github.com/stretchr/piglatin")
	//return cmd.Run()
	return nil
}

// Clean up after yourself
func Clean() {
	b := buildArtifacts()

	filesToDelete := []string{b.CompiledApp, b.InstalledApp}

	fmt.Println("Cleaning: ")

	for _, f := range filesToDelete {
		fmt.Printf("  > deleting ")
		fmt.Print(f)
		fmt.Println()

		err := os.RemoveAll(b.CompiledApp)
		if err != nil {
			fmt.Printf("    ! there was a problem deleting %s: %s", f, err)
		}
	}
}
