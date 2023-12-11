// Package root will build the root cobra.Command for the mlrb app.
package root

import (
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/command/lbr"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/context"
)

const (
	DebugFlagParam    = "debug"
	TimeZoneFlagParam = "timezone"
)

// NewRootCmd creates the root Cobra command that is the entry point to all the things in the application.
func NewRootCmd(cfg *config.Config) *cobra.Command {

	rootCmd := &cobra.Command{
		Use:              "mlrb <command> <subcommand> [flags]",
		Short:            "MyLittleRangeBook CLI",
		Long:             "Testing ground for learning Go and managing data around handloading.",
		TraverseChildren: true,
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			return command.ConfigureCmd(cmd)
		},
	}

	rootCmd.SetOut(cfg.Out)
	rootCmd.SetIn(cfg.In)
	rootCmd.SetErr(cfg.Err)

	rootCmd.PersistentFlags().BoolVarP(&cfg.Debug, DebugFlagParam, "", false, "Set to true to enable debugging (for development only).")
	rootCmd.PersistentFlags().StringVarP(&cfg.Timezone, TimeZoneFlagParam, "", context.InferTimeZone(), "Set the timezone for the Labradar.")

	rootCmd.AddCommand(lbr.NewLabradarCmd(cfg))

	return rootCmd
}
