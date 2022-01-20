// Package cmd holds the code to configure the various cobra.Command structures for an app.
package cmd

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"github.com/spf13/pflag"
	"github.com/spf13/viper"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"strings"
)

// SetMandatoryFlags Sets the specified flags as mandatory.  This is a helper method to reduce some of the repetitiveness with
// setting mandatory flags. If there is an error setting the mandatory flag, then a warning would be logged.
func SetMandatoryFlags(cmd *cobra.Command, flagnames ...string) {
	type f struct {
		flagName string
		success  bool
		c        *cobra.Command
	}

	flags := make([]f, len(flagnames))
	for _, n := range flagnames {
		err := cmd.MarkFlagRequired(n)
		flags = append(flags, f{flagName: n, success: err == nil, c: cmd})
		if err != nil {
			logrus.Warnf("Could not make the flag %s mandatory: %v.", n, err.Error())
		}
	}
}

func InitializeCommand(cmd *cobra.Command) error {
	v := viper.New()
	v.SetConfigName(config.CONFIG_FILENAME)
	v.AddConfigPath(".") // We only care about a config file in the current directory.

	// Attempt to read the config file, gracefully ignoring errors caused by a config file not being found. Return an error
	// if we cannot parse the config file.
	if err := v.ReadInConfig(); err != nil {
		if _, ok := err.(viper.ConfigFileNotFoundError); !ok {
			// It's okay if there isn't a config file. Otherwise we returnthe error.
			return err
		}
	}

	// When we bind flags to environment variables expect that the environment variables are prefixed, e.g. a flag like
	// --number binds to an environment variable MLRB_NUMBER. This helps avoid conflicts.
	v.SetEnvPrefix(config.CONFIG_ENVIRONMENT_VARIABLE_PREFIX)

	// Bind to environment variables. Works great for simple config names, but needs help for names like --favorite-color
	// which we fix in the BindFlags function
	v.AutomaticEnv()
	BindFlags(cmd, v) // Bind the current command's flags to viper

	return nil
}

func BindFlags(cmd *cobra.Command, v *viper.Viper) {

	var flags = cmd.Flags()
	flags.VisitAll(func(f *pflag.Flag) {

		// Environment variables can't have dashes in them, so bind them to their equivalent
		// keys with underscores, e.g. --favorite-color to STING_FAVORITE_COLOR
		if strings.Contains(f.Name, "-") {
			envVarSuffix := strings.ToUpper(strings.ReplaceAll(f.Name, "-", "_"))
			envVariableName := fmt.Sprintf("%s_%s", config.CONFIG_ENVIRONMENT_VARIABLE_PREFIX, envVarSuffix)
			_ = v.BindEnv(f.Name, envVariableName)
		}

		// Apply the viper config value to the flag when the flag is not set and viper has a value
		getValueFromViper := !f.Changed && v.IsSet(f.Name)
		if getValueFromViper {
			val := v.Get(f.Name)
			err := cmd.Flags().Set(f.Name, fmt.Sprintf("%v", val))
			if err != nil {
			}
		}
	})
}
