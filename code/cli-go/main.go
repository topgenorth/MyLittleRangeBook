package main

import (
	"fmt"
	log "github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"github.com/spf13/pflag"
	"github.com/spf13/viper"
	//"opgenorth.net/mlrb/pkg/config"
	//"opgenorth.net/mlrb/pkg/labradar"
	//"opgenorth.net/mlrb/pkg/mlrb"
	"os"
	"strings"
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
			return initializeCommand(cmd)
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

func initializeCommand(cmd *cobra.Command) error {
	v := viper.New()

	// Set the base name of the config file, without the file extension.
	v.SetConfigName(config.Filename)

	// Set as many paths as you like where viper should look for the
	// config file. We are only looking in the current working directory.
	v.AddConfigPath(".")

	// Attempt to read the config file, gracefully ignoring errors
	// caused by a config file not being found. Return an error
	// if we cannot parse the config file.
	if err := v.ReadInConfig(); err != nil {
		// It's okay if there isn't a config file
		if _, ok := err.(viper.ConfigFileNotFoundError); !ok {
			return err
		}
	}

	// When we bind flags to environment variables expect that the
	// environment variables are prefixed, e.g. a flag like --number
	// binds to an environment variable STING_NUMBER. This helps
	// avoid conflicts.
	v.SetEnvPrefix(config.EnvPREFIX)

	// Bind to environment variables
	// Works great for simple config names, but needs help for names
	// like --favorite-color which we fix in the bindFlags function
	v.AutomaticEnv()

	// Bind the current command's flags to viper
	bindFlags(cmd, v)

	return nil
}

func buildListCartridgesCmd(a *mylittlerangebook.MyLittleRangeBook) *cobra.Command {
	cmd := &cobra.Command{
		Use:   "listcartridges",
		Short: "List the cartridges in Amplify",
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
		Config:       app.Config,
		SeriesNumber: 0,
		InputDir:     "",
		OutputDir:    "",
	}

	cmd := &cobra.Command{
		Use:   "readcsv",
		Short: "Reads a Device CSV file and converts it to JSON.",
		Long:  `Currently this will read a CSV file and convert it to JSON.`,
		Run: func(cmd *cobra.Command, args []string) {
			series, err := app.ReadLabradarCsv(readCsvCfg)
			if err != nil {
				log.Fatal(err)
			}
			series.Print()
		},
	}

	// Define cobra flags, the default value has the lowest (least significant) precedence
	cmd.Flags().IntVarP(&readCsvCfg.SeriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
	cmd.Flags().StringVarP(&readCsvCfg.InputDir, "labradar.inputDir", "i", "", "The location of the input files.")
	cmd.Flags().StringVarP(&readCsvCfg.OutputDir, "labradar.outputDir", "o", "", "The location of the output files.")
	return cmd
}

// Bind each cobra flag to its associated viper configuration (config file and environment variable)
func bindFlags(cmd *cobra.Command, v *viper.Viper) {
	cmd.Flags().VisitAll(func(f *pflag.Flag) {
		// Environment variables can't have dashes in them, so bind them to their equivalent
		// keys with underscores, e.g. --favorite-color to STING_FAVORITE_COLOR
		if strings.Contains(f.Name, "-") {
			envVarSuffix := strings.ToUpper(strings.ReplaceAll(f.Name, "-", "_"))
			v.BindEnv(f.Name, fmt.Sprintf("%s_%s", config.EnvPREFIX, envVarSuffix))
		}

		// Apply the viper config value to the flag when the flag is not set and viper has a value
		if !f.Changed && v.IsSet(f.Name) {
			val := v.Get(f.Name)
			cmd.Flags().Set(f.Name, fmt.Sprintf("%v", val))
		}
	})
}
