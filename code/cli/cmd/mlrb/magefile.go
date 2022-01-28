//go:build mage
// +build mage

package main

import (
	"fmt"
	"os"
	"os/exec"

	mg "github.com/magefile/mage/mg"
)

const (
	compiledApp = "C:\\Users\\tom.opgenorth\\code\\MyLittleRangeBook\\bin\\mlrb.exe"
)

// A build step that requires additional params, or platform specific steps for example
func Build() error {
	mg.Deps(InstallDeps)
	fmt.Printf("Building %s...\n", compiledApp)
	cmd := exec.Command("go", "build", "-o", compiledApp, "C:\\Users\\tom.opgenorth\\code\\MyLittleRangeBook\\code\\cli\\cmd\\mlrb\\main.go")
	return cmd.Run()
}

// A custom install step if you need your bin someplace other than go/bin
func Install() error {
	mg.Deps(Build)
	fmt.Printf("Installing %s...\n", compiledApp)
	return os.Rename(compiledApp, "C:\\Users\\tom.opgenorth\\code\\MyLittleRangeBook\\data\\mlrb.exe")
}

// Manage your deps, or running package managers.
func InstallDeps() error {
	fmt.Println("Installing Deps...")
	//cmd := exec.Command("go", "get", "github.com/stretchr/piglatin")
	//return cmd.Run()
	return nil
}

// Clean up after yourself
func Clean() {
	fmt.Println("Cleaning")
	fmt.Printf("  > deleting %s\n", compiledApp)
	err := os.RemoveAll(compiledApp)
	if err != nil {
		return
	}
}
