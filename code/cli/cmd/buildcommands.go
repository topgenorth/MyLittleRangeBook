package cmd

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"github.com/spf13/pflag"
	"github.com/spf13/viper"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
	"strings"
)

// BuildRootCmd creates the root Cobra command that is the entry point to all the things in the application.
func BuildRootCmd() *cobra.Command {
	app := mlrb.New()
	app.ConfigLogging()

	rootCmd := &cobra.Command{
		Use:  "mlrb [command]",
		Long: "mlrb is my app for the various reloading things.",
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			return initializeCommand(cmd)
		},
		RunE: func(cmd *cobra.Command, args []string) error {
			return cmd.Help()
		},
	}

	//rootCmd.AddCommand(buildCartridgeCommands(app))
	rootCmd.AddCommand(buildLabradarCommands(app))

	return rootCmd
}

func initializeCommand(cmd *cobra.Command) error {
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
	// which we fix in the bindFlags function
	v.AutomaticEnv()
	bindFlags(cmd, v) // Bind the current command's flags to viper

	return nil
}

func bindFlags(cmd *cobra.Command, v *viper.Viper) {
	cmdName := fmt.Sprintf("'%s'", cmd.Name())

	logrus.Tracef("Binding flags for the command %s.", cmdName)

	var flags = cmd.Flags()
	flags.VisitAll(func(f *pflag.Flag) {

		logrus.Tracef("Processing the flag %s on command %s.", f.Name, cmdName)

		// Environment variables can't have dashes in them, so bind them to their equivalent
		// keys with underscores, e.g. --favorite-color to STING_FAVORITE_COLOR
		if strings.Contains(f.Name, "-") {
			envVarSuffix := strings.ToUpper(strings.ReplaceAll(f.Name, "-", "_"))
			envVariableName := fmt.Sprintf("%s_%s", config.CONFIG_ENVIRONMENT_VARIABLE_PREFIX, envVarSuffix)
			logrus.Tracef("Trying to bind the flag %s to the environment variable %s.", f.Name, envVariableName)

			err := v.BindEnv(f.Name, envVariableName)
			if err != nil {
				logrus.Tracef("   > Error tring to bind the flag %s to the environment variable %s: %v.", f.Name, envVariableName, err)
			}
		} else {
			logrus.Tracef("   > Not binding the flag %s to environment variables.", f.Name)
		}

		// Apply the viper config value to the flag when the flag is not set and viper has a value
		getValueFromViper := !f.Changed && v.IsSet(f.Name)
		if getValueFromViper {
			val := v.Get(f.Name)
			err := cmd.Flags().Set(f.Name, fmt.Sprintf("%v", val))
			if err != nil {
				logrus.Tracef("   > Error trying to the value for %s from : %v", err)
			}

		} else {
			logrus.Tracef("   > Not setting the flag %s on the command", f.Name)
		}
	})
}

// Sets the specified flags as mandatory.  This is a helper method to reduce some of the repetitiveness with
// setting mandatory flags. If there is an error setting the mandatory flag, then a warning would be logged.
func setMandatoryFlags(cmd *cobra.Command, flagnames ...string) {
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
