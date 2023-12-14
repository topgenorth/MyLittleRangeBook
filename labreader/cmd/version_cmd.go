package cmd

import (
	"fmt"

	"github.com/spf13/cobra"
)

var version = "0.1.0"

// versionCmd represents the version command
var versionCmd = &cobra.Command{
	Use:   "version",
	Short: "Display the current version.",
	Run: func(cmd *cobra.Command, args []string) {
		fmt.Println("labreader version:", version)
	},
}

func init() {
	rootCmd.AddCommand(versionCmd)
}
