package commands

import (
	"fmt"
	"github.com/spf13/cobra"
	"opgenorth.net/labradar/pkg/config"
	"time"
)

func GetReadmeCmd() *cobra.Command {

	inputDirectory := ""
	outputDirectory := ""
	timezone := ""

	cmd := &cobra.Command{
		Use:   "readme",
		Short: "Loads the README.md an adds it to the relevant JSON file.",
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			// You can bind cobra and viper in a few locations, but PersistencePreRunE on the root command works well
			return initializeConfig(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {
			cfg := config.New()
			cfg.InputDir = inputDirectory
			cfg.OutputDir = outputDirectory
			cfg.TimeZone, _ = time.LoadLocation(timezone)

			fmt.Println("Should be parsing the README file in " + inputDirectory)
		},
	}

	cmd.Flags().StringVarP(&inputDirectory, "inputDir", "i", "", "The location of the input files.")
	cmd.Flags().StringVarP(&outputDirectory, "outputDir", "o", "", "The location of the output files.")

	return cmd
}
