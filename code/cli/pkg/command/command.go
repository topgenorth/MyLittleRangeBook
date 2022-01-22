// Package command holds the code to configure the various cobra.Command structures for an app.
package command

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

// ConfigureCmd will setup the flags for a given cobra.Command.
// The priority is:
//   1. command line
//   2. environment variables
//   3. configuration file (mlrb.toml)
func ConfigureCmd(cmd *cobra.Command) error {

	v := releaseTheViper()

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
	v.SetEnvPrefix(config.MlrbEnvironmentVariablePrefix)

	// Bind to environment variables. Works great for simple config names, but needs help for names like --favorite-color
	// which we fix in the bindFlags function
	v.AutomaticEnv()
	bindFlags(cmd, v) // Bind the current command's flags to viper

	return nil
}

func releaseTheViper() *viper.Viper {
	v := viper.New()
	v.AddConfigPath(".")
	v.SetConfigName(config.MlrbConfigFileName)
	v.SetConfigType(config.MlrbConfigFileType)
	return v
}

func bindFlags(cmd *cobra.Command, v *viper.Viper) {

	var flagSet = cmd.Flags()

	flagSet.VisitAll(func(f *pflag.Flag) {

		// Environment variables can't have dashes in them, so bind them to their equivalent
		// keys with underscores, e.g. --favorite-color to STING_FAVORITE_COLOR
		if strings.Contains(f.Name, "-") {
			envVarSuffix := strings.ToUpper(strings.ReplaceAll(f.Name, "-", "_"))
			envVariableName := fmt.Sprintf("%s_%s", config.MlrbEnvironmentVariablePrefix, envVarSuffix)

			logrus.Tracef("envVarSuffix='%s'; envVariableName='%s'", envVarSuffix, envVariableName)
			if err := v.BindEnv(f.Name, envVariableName); err != nil {
				logrus.Tracef("Will not get the value '%s' for command %s from environment variables.", f.Name, getType(cmd))
			}

		}

		// Apply the viper config value to the flag when the flag is not set and viper has a value
		getValueFromViper := !f.Changed && v.IsSet(f.Name)
		if getValueFromViper {
			val := v.Get(f.Name)
			err := cmd.Flags().Set(f.Name, fmt.Sprintf("%v", val))
			if err != nil {
				logrus.Errorf("Trying to get the value '%s' for the command %s from config: %v", f.Name, err, getType(cmd))
			}
		} else {
			logrus.Errorf("Will not get the value '%s' for the command %s from config.", f.Name, getType(cmd))
		}
	})
}

func getType(cmd *cobra.Command) string {

	return fmt.Sprintf("`%s`", cmd.Name())

	//return reflect.TypeOf(myvar).String()
	//if t := reflect.TypeOf(myvar); t.Kind() == reflect.Ptr {
	//	return "*" + t.Elem().Name()
	//} else {
	//	return t.Name()
	//}
}
