// Package main is the entry point for the mlrb app.
package main

import (
	"opgenorth.net/mylittlerangebook/pkg/cmd/root"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
	"os"
)

type exitCode int

const (
	exitOK    exitCode = 0
	exitError exitCode = 1
	//exitCancel exitCode = 2
	//exitAuth   exitCode = 4
)

func main() {
	code := mainRun()
	os.Exit(int(code))
}

func mainRun() exitCode {
	app := mlrb.New()
	rootCmd := root.NewRootCmd(app)

	if err := rootCmd.Execute(); err != nil {
		return exitError
	}

	return exitOK
}

// based on https://github.com/watson/ci-info/blob/HEAD/index.js
func isCI() bool {
	return os.Getenv("CI") != "" || // GitHub Actions, Travis CI, CircleCI, Cirrus CI, GitLab CI, AppVeyor, CodeShip, dsari
		os.Getenv("BUILD_NUMBER") != "" || // Jenkins, TeamCity
		os.Getenv("RUN_ID") != "" // TaskCluster, dsari
}
