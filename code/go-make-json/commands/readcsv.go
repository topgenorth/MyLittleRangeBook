/*
Copyright © 2021 Tom Opgenorth <tom@opgenorth.net>

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
package commands

import (
	"fmt"
	"github.com/spf13/cobra"
	"opgenorth.net/labradar/pkg/config"
	"opgenorth.net/labradar/pkg/labradar"
	"time"
)

func ReadLabradarFileCmd() *cobra.Command {
	seriesNumber := 0
	inputDirectory := ""
	outputDirectory := ""
	timezone := ""

	cmd := &cobra.Command{
		Use:   "readcsv",
		Short: "Reads a Device CSV file and converts it to JSON.",
		Long:  `Currently this will read a CSV file and convert it to JSON.`,
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			// You can bind cobra and viper in a few locations, but PersistencePreRunE on the root command works well
			return initializeConfig(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {

			cfg := config.New()
			cfg.InputDir = inputDirectory
			cfg.OutputDir = outputDirectory
			cfg.TimeZone, _ = time.LoadLocation(timezone)

			ls := labradar.NewSeries(seriesNumber, cfg)
			err := labradar.LoadLabradarSeriesFromCsv(ls, cfg)
			if err != nil {
				return
			}

			err2 := labradar.SaveLabradarSeriesToJson(ls, cfg)
			if err2 != nil {
				fmt.Println(err2)
			}
		},
	}

	// Define cobra flags, the default value has the lowest (least significant) precedence
	cmd.Flags().IntVarP(&seriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
	cmd.Flags().StringVarP(&inputDirectory, "inputDir", "i", "", "The location of the input files.")
	cmd.Flags().StringVarP(&outputDirectory, "outputDir", "o", "", "The location of the output files.")
	cmd.Flags().StringVarP(&timezone, "timezone", "", "", "The IANA timezone that the Labradar is in.")
	return cmd
}
