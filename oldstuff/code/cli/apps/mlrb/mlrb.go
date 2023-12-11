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
