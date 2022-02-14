// Package main is the entry point for the mlrb app.
package main

import (
	"github.com/sirupsen/logrus"
	"opgenorth.net/mylittlerangebook/pkg/command/root"
	"opgenorth.net/mylittlerangebook/pkg/config"
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
	code := runMyLittleRangeBook()
	os.Exit(int(code))
}

func runMyLittleRangeBook() exitCode {

	c := config.New()
	rootCmd := root.NewRootCmd(c)

	if err := rootCmd.Execute(); err != nil {
		logrus.WithError(err).Errorln("Well, that didn't go well.")
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
