// Package root will build the root cobra.Command for the mlrb app.
package root

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/command/lbr"
	"opgenorth.net/mylittlerangebook/pkg/config"
)

const (
	DebugFlagParam = "debug"
)

type RootCommandOptions struct {
	Debug bool
}

// NewRootCmd creates the root Cobra command that is the entry point to all the things in the application.
func NewRootCmd(cfg *config.Config) *cobra.Command {

	opts := RootCommandOptions{}
	rootCmd := &cobra.Command{
		Use:   "mlrb <command> <subcommand> [flags]",
		Short: "MyLittleRangeBook CLI",
		Long:  "Testing ground for learning Go and managing data around handloading.",
	}

	rootCmd.SetOut(cfg.Out)
	rootCmd.SetIn(cfg.In)
	rootCmd.SetErr(cfg.Err)

	rootCmd.PersistentFlags().BoolVarP(&opts.Debug, DebugFlagParam, "", false, "Set to true to enable debugging (for development only).")

	err := command.ConfigureCmd(rootCmd)

	if err != nil {
		logrus.Panicf("Could not initialize the root command: %v", err)
		return nil
	}

	rootCmd.AddCommand(lbr.NewLabradarCmd(cfg))

	return rootCmd
}
