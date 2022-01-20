package root

import (
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/cmd"
	"opgenorth.net/mylittlerangebook/pkg/cmd/labradar"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

// NewRootCmd creates the root Cobra command that is the entry point to all the things in the application.
func NewRootCmd() *cobra.Command {
	app := mlrb.New()

	rootCmd := &cobra.Command{
		Use:  "mlrb [command]",
		Long: "mlrb is my app for the various reloading things.",
		PersistentPreRunE: func(cc *cobra.Command, args []string) error {
			return cmd.InitializeCommand(cc)
		},
		RunE: func(cmd *cobra.Command, args []string) error {
			return cmd.Help()
		},
	}

	//rootCmd.AddCommand(buildCartridgeCommands(app))
	rootCmd.AddCommand(labradar.BuildLabradarCommands(app))

	return rootCmd
}
