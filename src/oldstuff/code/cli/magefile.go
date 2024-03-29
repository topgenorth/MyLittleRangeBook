//go:build mage
// +build mage

package main

import (
	"fmt"
	"github.com/magefile/mage/mg"
	"github.com/magefile/mage/sh"
	"github.com/sirupsen/logrus"
	"opgenorth.net/mylittlerangebook/build"
	"opgenorth.net/mylittlerangebook/pkg/fs"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
)

// allow user to override go executable by running as GOEXE=xxx make ... on unix-like systems
var goexe = "go"

func init() {
	if exe := os.Getenv("GOEXE"); exe != "" {
		goexe = exe
	}

	// We want to use Go 1.11 modules even if the source lives inside GOPATH.
	// The default is "auto".
	err := os.Setenv("GO111MODULE", "on")
	if err != nil {
		logrus.WithError(err).Warningln("There was a problem trying to set the environment variable GO111MODULE to `on`.")
	}
}

// Manage your deps, or running package managers.
func InstallDeps() error {
	fmt.Println("This doesn't actually do anything right now.")
	//b := buildArtifacts()

	//cmd := exec.Command("go", "get", "github.com/stretchr/piglatin")
	//return cmd.Run()
	return nil
}

// A build step that requires additional params, or platform specific steps for example
func Build() error {
	b := buildArtifacts()

	mg.Deps(InstallDeps)
	fmt.Printf("Compiling %s to %s...\n", b.MainGo, b.CompiledApp)
	cmd := exec.Command("go", "build", "-v", "-x", "-o", b.CompiledApp, b.MainGo)
	return cmd.Run()
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

	fmt.Printf("Installing %s to %s\n", b.CompiledApp, b.InstalledApp)
	return fs.CopyFile(b.CompiledApp, b.InstalledApp)
}

// SeedData will add some test data to the SQLite database.

// Run tests
func Test() error {
	env := make(map[string]string)
	return runCmd(env, goexe, "test", "./...")
}

// Clean up after yourself
func Clean() {
	b := buildArtifacts()

	filesToDelete := []string{b.CompiledApp, b.InstalledApp, dataMlrbExe(b)}

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

func buildArtifacts() build.Artifacts {
	currentDir, err := fs.CurrentWd()
	if err != nil {
		logrus.WithError(err).Panicf("Problem figuring out our build environment.")
	}
	b := build.New(currentDir)
	return b
}

func dataMlrbExe(b build.Artifacts) string {

	dataExe := filepath.Join(b.CurrentDir, "..", "..", "data", b.ExecutableName)
	return dataExe
}

// based on https://github.com/watson/ci-info/blob/HEAD/index.js
func isCI() bool {
	return os.Getenv("CI") != "" || // GitHub Actions, Travis CI, CircleCI, Cirrus CI, GitLab CI, AppVeyor, CodeShip, dsari
		os.Getenv("BUILD_NUMBER") != "" || // Jenkins, TeamCity
		os.Getenv("RUN_ID") != "" // TaskCluster, dsari
}

func buildFlags() []string {
	if runtime.GOOS == "windows" {
		return []string{"-buildmode", "exe"}
	}
	return nil
}

func runCmd(env map[string]string, cmd string, args ...interface{}) error {
	if mg.Verbose() {
		return runWith(env, cmd, args...)
	}
	output, err := sh.OutputWith(env, cmd, argsToStrings(args...)...)
	if err != nil {
		_, _ = fmt.Fprint(os.Stderr, output)
	}

	return err
}

func runWith(env map[string]string, cmd string, inArgs ...interface{}) error {
	s := argsToStrings(inArgs...)
	return sh.RunWith(env, cmd, s...)
}

func argsToStrings(v ...interface{}) []string {
	var args []string
	for _, arg := range v {
		switch v := arg.(type) {
		case string:
			if v != "" {
				args = append(args, v)
			}
		case []string:
			if v != nil {
				args = append(args, v...)
			}
		default:
			panic("invalid type")
		}
	}

	return args
}
