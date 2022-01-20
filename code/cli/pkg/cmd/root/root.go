// Package root will build the root cobra.Command for the mlrb app.
package root

import (
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/cmd/lbr"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

// NewRootCmd creates the root Cobra command that is the entry point to all the things in the application.
func NewRootCmd(a *mlrb.MyLittleRangeBook) *cobra.Command {

	rootCmd := &cobra.Command{
		Use:   "mlrb <command> <subcommand> [flags]",
		Short: "MyLittleRangeBook CLI",
		Long:  "Testing ground for learning Go and managing data around handloading.",
	}

	rootCmd.SetOut(a.Out)
	rootCmd.SetIn(a.In)
	rootCmd.SetErr(a.Err)

	rootCmd.AddCommand(lbr.NewLabradarCmd(a))

	return rootCmd
}
