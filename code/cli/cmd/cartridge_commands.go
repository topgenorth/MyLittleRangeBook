package cmd

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

func buildCartridgeCommands(a *mlrb.MyLittleRangeBook) *cobra.Command {
	cmd := &cobra.Command{
		Use:   "cartridge",
		Short: "All the commands for dealing with cartridges via the command line.",
	}

	cmd.AddCommand(buildListCartridgesCmd(a))
	cmd.AddCommand(buildAddCartridgeCommand(a))

	return cmd
}

func buildListCartridgesCmd(a *mlrb.MyLittleRangeBook) *cobra.Command {
	cmd := &cobra.Command{
		Use:   "list",
		Short: "List the cartridges in the datastore.",
		Run: func(cmd *cobra.Command, args []string) {
			a.ConfigLogging()
			a.ListCartridges()
		},
	}

	return cmd
}

func buildAddCartridgeCommand(a *mlrb.MyLittleRangeBook) *cobra.Command {
	var (
		name string
		size string
	)
	cmd := &cobra.Command{
		Use:   "add",
		Short: "Add a new cartridge to the list.",
		Run: func(cmd *cobra.Command, args []string) {

			c2, err := a.SubmitCartridge(name, size)
			if err != nil {
				logrus.Fatal(err)
			}

			logrus.Infof("Added %s to the list.", c2)
		},
	}
	cmd.Flags().StringVarP(&name, "name", "n", "", "A unique name for the cartridge.")
	cmd.Flags().StringVarP(&size, "size", "s", "", "The size of the cartridge (metric).")
	return cmd
}
