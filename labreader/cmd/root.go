package cmd

import (
	"fmt"
	"github.com/spf13/cobra"
	"github.com/spf13/viper"
	"labreader/cmd/catalog"
	"labreader/internal/logger"
	"os"
)

var (
	configFile  string
	environment string
)

// rootCmd represents the base command when called without any subcommands
var rootCmd = &cobra.Command{
	Use:   "labreader",
	Short: "Read the velocities from a Labradar.",
	Long:  `My third attempt at this; this is mostly a project for me to learn Go.`,
}

func Execute() {
	if err := rootCmd.Execute(); err != nil {
		fmt.Println(err)
		os.Exit(1)
	}
}

func init() {
	cobra.OnInitialize(initConfig)

	rootCmd.PersistentFlags().StringVarP(&configFile, "config", "c", configFile, "The configuration file for this application.")
	_ = viper.BindPFlag("config", rootCmd.PersistentFlags().Lookup("config"))
	viper.SetDefault("config", getDefaultConfigDirectory())

	rootCmd.PersistentFlags().StringVarP(&environment, "environment", "e", "Production", "The environment.  Production or Development.")
	_ = viper.BindPFlag("environment", rootCmd.PersistentFlags().Lookup("environment"))
	viper.SetDefault("environment", "Development")

	rootCmd.PersistentFlags().BoolP("dryrun", "", false, "Dry run - won't do anything.")
	_ = viper.BindPFlag("dryrun", rootCmd.PersistentFlags().Lookup("dryrun"))
	viper.SetDefault("dryrun", false)

	rootCmd.AddCommand(catalog.NewCatalogCommand())
}

func initConfig() {
	if configFile != "" {
		viper.SetConfigFile(configFile)
	} else {
		viper.AddConfigPath(getDefaultConfigDirectory())
		viper.SetConfigType("yaml")
		viper.SetConfigName(".labreader")
	}

	viper.AutomaticEnv()
	viper.SetEnvPrefix("LBR")

	if err := viper.ReadInConfig(); err == nil {
		logger.DefaultLogger().
			Debug().
			Str("config_file", viper.ConfigFileUsed()).
			Send()
	}

}

func getDefaultConfigDirectory() string {
	home, err := os.UserHomeDir()
	cobra.CheckErr(err)
	if err != nil {
		logger.DefaultLogger().Fatal().
			Err(err).
			Send()
	}
	return home
}
