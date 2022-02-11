package cartridge

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
	"opgenorth.net/mylittlerangebook/pkg/persistence"
)

func NewCartridgeCommand(cfg *config.Config) *cobra.Command {
	c := &cobra.Command{
		Use:   "cartridge",
		Short: "All the command for dealing with cartridges via the command line.",
	}

	//c.AddCommand(buildListCartridgesCmd(a))
	c.AddCommand(buildAddCartridgeCommand(cfg))

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

func buildAddCartridgeCommand(cfg *config.Config) *cobra.Command {
	var (
		name string
		size string
	)
	c := &cobra.Command{
		Use:   "add",
		Short: "Add a new cartridge to the list.",
		RunE: func(cmd *cobra.Command, args []string) error {
			return saveCartridge(cfg, name, size)
		},
	}
	c.Flags().StringVarP(&name, "name", "", "", "A unique name for the cartridge.")
	c.Flags().StringVarP(&size, "size", "", "", "The size of the cartridge (metric).")
	command.SetMandatoryFlags(c, "name", "size")

	return c
}

func saveCartridge(cfg *config.Config, name string, size string) error {

	err := persistence.Bootstrap()
	if err != nil {
		return err
	}

	a := mlrb.New(cfg)

	if err = a.SaveCartridgeToSqlLite(name, size); err != nil {
		return err
	}

	logrus.Tracef("Saved %s, %s to the Sqlite.", name, size)
	return nil
}
