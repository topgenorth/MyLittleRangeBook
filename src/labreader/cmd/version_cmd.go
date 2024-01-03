package cmd

import (
	"fmt"

	"github.com/spf13/cobra"
)

const LabreaderVersion = "0.2.0"

// versionCmd represents the version command
var versionCmd = &cobra.Command{
	Use:   "version",
	Short: "Display the current version.",
	Run: func(cmd *cobra.Command, args []string) {
		fmt.Println("LabReader version: " + LabreaderVersion)
	},
}
