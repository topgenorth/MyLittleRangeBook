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
package main

import (
	"fmt"
	log "github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/labradar/pkg/labradar"
	"opgenorth.net/labradar/pkg/mylittlerangebook"
	"os"
)

func main() {
	rootCmd := buildRootCommand()

	if err := rootCmd.Execute(); err != nil {
		os.Exit(1)
	}
}



func buildRootCommand() *cobra.Command {
	app := mylittlerangebook.New()

	cmd := &cobra.Command{
		Use:  "mlrb [sub]",
		Long: "mlrb is my app for the various reloading things.",
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			err := app.ConfigCmd(cmd)
			if err != nil {
				return err
			}
			return nil
		},
		RunE: func(cmd *cobra.Command, args []string) error {
			return cmd.Help()
		},
	}

	cmd.PersistentFlags().BoolVar(&app.Debug, "debug", false, "Enable debug logging")
	cmd.PersistentFlags().StringVar(&app.Timezone, "timezone", "", "Set the timezone of the Labradar file.")

	cmd.AddCommand(buildReadLabradarFileCmd(app))
	cmd.AddCommand(buildListCartridgesCmd(app))
	cmd.AddCommand(buildInitMyLittleRangeBookCmd(app))
	return cmd
}

func buildListCartridgesCmd(a *mylittlerangebook.MyLittleRangeBook) *cobra.Command {
	cmd := &cobra.Command{
		Use:   "listcartridges",
		Short: "List the cartridges in Amplify",
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			return a.ConfigCmd(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {
			a.ConfigLogging()
			a.ListCartridges()
		},
	}

	return cmd
}

func buildInitMyLittleRangeBookCmd(app *mylittlerangebook.MyLittleRangeBook) *cobra.Command {
	var homeDir string
	cmd := &cobra.Command{
		Use:   "init",
		Short: "Will initialize a directory for use.",
		Long:  "Will setup the directory for string files.",
		PreRunE: func(cmd *cobra.Command, args []string) error {
			return app.ConfigCmd(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {
			app.Init(homeDir)
			log.Debugf("Initialized the directory %s.", homeDir)
		},
	}
	cmd.Flags().StringVarP(&homeDir, "homeDir", "", "", "The location of the home directory.")

	return cmd
}

func buildReadLabradarFileCmd(app *mylittlerangebook.MyLittleRangeBook) *cobra.Command {
	readCsvCfg := &labradar.ReadCsvConfig{
		SeriesNumber: 0,
		InputDir:     "",
		OutputDir:    "",
	}

	cmd := &cobra.Command{
		Use:   "readcsv",
		Short: "Reads a Device CSV file and converts it to JSON.",
		Long:  `Currently this will read a CSV file and convert it to JSON.`,
		PreRunE: func(cmd *cobra.Command, args []string) error {
			return app.ConfigCmd(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {
			app.ConfigLogging()
			series, err := app.ReadLabradarCsv(readCsvCfg)
			if err != nil {
				log.Fatal(err)
			}

			fmt.Print(series)
		},
	}

	// Define cobra flags, the default value has the lowest (least significant) precedence
	cmd.Flags().IntVarP(&readCsvCfg.SeriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
	cmd.Flags().StringVarP(&readCsvCfg.InputDir, "inputDir", "i", "", "The location of the input files.")
	cmd.Flags().StringVarP(&readCsvCfg.OutputDir, "outputDir", "o", "", "The location of the output files.")
	return cmd
}
