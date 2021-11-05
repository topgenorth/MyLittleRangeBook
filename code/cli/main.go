package main

import (
	"opgenorth.net/mylittlerangebook/cmd"
	"os"
)

func main() {
	rootCmd := cmd.BuildRootCmd()

	if err := rootCmd.Execute(); err != nil {
		os.Exit(1)
	}
}
