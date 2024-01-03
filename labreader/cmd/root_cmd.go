package cmd

import (
	"errors"
	"github.com/rs/zerolog"
	"github.com/spf13/cobra"
	"github.com/spf13/viper"
	"labreader/cmd/catalog"
	"labreader/cmd/readstats"
	"labreader/internal/logger"
	"os"
	"path"
)

var (
	configFile  string
	environment string
	l           zerolog.Logger
)

// rootCmd represents the base command when called without any subcommands
var rootCmd = &cobra.Command{
	Use:   "labreader",
	Short: "Read the velocities from a Labradar.",
	Long:  `My third attempt at this; this is mostly a project for me to learn Go.`,
}

func init() {
	// TODO [TO20231215] Currently logging to the development environment and debug.
	l = logger.New(logger.DevelopmentEnvironment(), logger.LogLevelDebug())

	cobra.OnInitialize(initCobraAndViper)
	rootCmd.AddCommand(versionCmd)
	rootCmd.AddCommand(catalog.NewCatalogCommand())
	rootCmd.AddCommand(readstats.NewReadStatsCommand())

	configCliFlags(rootCmd)
}

func configCliFlags(cmd *cobra.Command) *cobra.Command {
	cmd.PersistentFlags().StringVarP(&configFile, "config", "c", configFile, "The configuration file for this application.")
	_ = viper.BindPFlag("config", rootCmd.PersistentFlags().Lookup("config"))
	viper.SetDefault("config", getDefaultConfigDirectory())

	cmd.PersistentFlags().StringVarP(&environment, "environment", "e", "Production", "The environment.  Production or Development.")
	_ = viper.BindPFlag("environment", rootCmd.PersistentFlags().Lookup("environment"))
	viper.SetDefault("environment", "Development")

	cmd.PersistentFlags().BoolP("dryrun", "", false, "Dry run - won't do anything.")
	_ = viper.BindPFlag("dryrun", rootCmd.PersistentFlags().Lookup("dryrun"))
	viper.SetDefault("dryrun", false)

	return cmd
}

func initCobraAndViper() {
	if configFile != "" {
		viper.SetConfigFile(configFile)
	} else {
		viper.AddConfigPath(getDefaultConfigDirectory())
		viper.SetConfigType("yaml")
		viper.SetConfigName(".labreader")
		viper.AddConfigPath("$HOME/.labreader")
		viper.AddConfigPath(".")
	}

	viper.AutomaticEnv()
	viper.SetEnvPrefix("LBR")

	if err := viper.ReadInConfig(); err != nil {
		var configFileNotFoundError viper.ConfigFileNotFoundError
		if errors.As(err, &configFileNotFoundError) {
			l.Warn().Err(err).Msg("Config file not found")

			newYamlFile := path.Join(getDefaultConfigDirectory(), ".labreader.yml")
			l.Debug().Str("config-file", newYamlFile).Msg("Created a new config file.")
			err2 := viper.SafeWriteConfigAs(newYamlFile)
			if err2 != nil {
				l.Error().Err(err).Send()
			}
		}
	} else {
		l.Debug().Str("config-file", viper.ConfigFileUsed()).Send()
	}
}

func getDefaultConfigDirectory() string {
	home, err := os.UserHomeDir()
	cobra.CheckErr(err)
	if err != nil {
		l.Err(err)
		home = ""
	}
	return home
}

func Execute() {
	if err := rootCmd.Execute(); err != nil {
		l.Fatal().Err(err).Send()
		os.Exit(1)
	}
}
