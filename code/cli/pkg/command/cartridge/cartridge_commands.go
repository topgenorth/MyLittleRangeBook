package cartridge

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

func buildCartridgeCommands(a *mlrb.MyLittleRangeBook) *cobra.Command {
	c := &cobra.Command{
		Use:   "cartridge",
		Short: "All the command for dealing with cartridges via the command line.",
	}

	c.AddCommand(buildListCartridgesCmd(a))
	c.AddCommand(buildAddCartridgeCommand(a))

	return c
}

func buildListCartridgesCmd(a *mlrb.MyLittleRangeBook) *cobra.Command {
	c := &cobra.Command{
		Use:   "list",
		Short: "List the cartridges in the datastore.",
		Run: func(cmd *cobra.Command, args []string) {
			a.ListCartridges()
		},
	}

	return c
}

func buildAddCartridgeCommand(a *mlrb.MyLittleRangeBook) *cobra.Command {
	var (
		name string
		size string
	)
	c := &cobra.Command{
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
	c.Flags().StringVarP(&name, "name", "n", "", "A unique name for the cartridge.")
	c.Flags().StringVarP(&size, "size", "", "", "The size of the cartridge (metric).")
	command.SetMandatoryFlags(c, "name", "size")

	return c
}
