//go:build mage
// +build mage

package main

import (
	"fmt"
	"github.com/magefile/mage/mg"
	"github.com/magefile/mage/sh"
	"os"
	"runtime"
)

// allow user to override go executable by running as GOEXE=xxx make ... on unix-like systems
var goexe = mg.GoCmd()

func init() {
	if exe := os.Getenv("GOEXE"); exe != "" {
		goexe = exe
	}

	err := os.Setenv("GO111MODULE", "on")
	if err != nil {
		fmt.Println("There was a problem trying to set the environment variable GO111MODULE to `on`.")
	}
}

func InstallDeps() error {
	err := os.MkdirAll("./output/", os.ModePerm)
	if err != nil {
		return err
	}
	return nil
}

// A build step that requires additional params, or platform specific steps for example
func Build() error {
	fmt.Println("Building...")
	mg.Deps(InstallDeps)
	name := executableName()
	return sh.RunV(goexe, "build", "-v", "-o", "./output/"+name, "./main.go")
}

func executableName() string {
	name := "labreader"
	if runtime.GOOS == "windows" {
		name += ".exe"
	}
	return name
}

// A custom install step if you need your bin someplace other than go/bin
func Install() error {
	mg.Deps(Build)

	src := "./output/labreader"
	dest := "labreader"
	if runtime.GOOS == "windows" {
		src += ".exe"
		dest = `c:\bin\` + dest + ".exe"
	} else {
		return fmt.Errorf("having figure out the destination for this OS")
		//dest = `/Users/tom/bin` + dest
	}

	fmt.Println("Install to " + dest)
	return sh.Copy(dest, src)
}

// Run tests
func Test() error {

	return sh.RunV(goexe, "test", "-v", "./cmd/catalog/")

}

func Clean() {
	filesToDelete := []string{"./output/"}

	for _, f := range filesToDelete {
		sh.Rm(f)
	}
}

// hash returns the git hash for the current repo or "" if none.
func hash() string {
	hash, _ := sh.Output("git", "rev-parse", "--short", "HEAD")
	return hash
}
