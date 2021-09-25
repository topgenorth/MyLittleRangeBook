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
	"github.com/spf13/cobra"
	"log"
	"opgenorth.net/labradar/pkg/mylittlerangebook"
	"os"
)

func main() {

	rootCmd := buildRootCommand()

	//rootCmd.AddCommand(
	//	cmd.ReadLabradarFileCmd(),
	//	cmd.ReadLabradarDirectoryCmd(),
	//	cmd.GetReadmeCmd(),
	//	cmd.ListCartridgesCmd(),
	//)

	if err := rootCmd.Execute(); err != nil {
		os.Exit(1)
	}
}

func buildRootCommand() *cobra.Command {
	a := mylittlerangebook.New()

	cmd := &cobra.Command{
		Use:  "mlrb [sub]",
		Long: "mlrb is my app for the various reloading things.",
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			err := a.ConfigCmd(cmd)
			if err != nil {
				return err
			}
			return nil
		},
		RunE: func(cmd *cobra.Command, args []string) error {
			return cmd.Help()
		},
	}

	cmd.PersistentFlags().BoolVar(&a.Debug, "debug", false, "Enable debug logging")
	cmd.PersistentFlags().StringVar(&a.Timezone, "timezone", "", "Set the timezone of the Labradar file.")

	cmd.AddCommand(buildReadLabradarFileCmd(a))
	cmd.AddCommand(buildListCartridgesCmd(a))

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
			a.ListCartridges()
		},
	}

	return cmd
}

func buildReadLabradarFileCmd(app *mylittlerangebook.MyLittleRangeBook) *cobra.Command {
	seriesNumber := 0
	inputDirectory := ""
	outputDirectory := ""

	cmd := &cobra.Command{
		Use:   "readcsv",
		Short: "Reads a Device CSV file and converts it to JSON.",
		Long:  `Currently this will read a CSV file and convert it to JSON.`,
		PreRunE: func(cmd *cobra.Command, args []string) error {
			return app.ConfigCmd(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {
			jsonFile, err := app.ConvertLabradarCsvToJson("")
			if err != nil {
				log.Fatal(err)
			}
			log.Println(jsonFile)
		},
	}

	// Define cobra flags, the default value has the lowest (least significant) precedence
	cmd.Flags().IntVarP(&seriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
	cmd.Flags().StringVarP(&inputDirectory, "inputDir", "i", "", "The location of the input files.")
	cmd.Flags().StringVarP(&outputDirectory, "outputDir", "o", "", "The location of the output files.")
	return cmd
}
