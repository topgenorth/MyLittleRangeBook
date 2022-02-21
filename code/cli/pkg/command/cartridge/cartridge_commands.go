package cartridge

import (
	"fmt"
	"github.com/spf13/cobra"
	"io"
	"log"
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

	c.AddCommand(buildListCartridgesCmd(cfg))
	c.AddCommand(buildSaveCartridgeCmd(cfg))
	c.AddCommand(buildDeleteCartridgeCmd(cfg))

	return c
}

func buildListCartridgesCmd(cfg *config.Config) *cobra.Command {
	c := &cobra.Command{
		Use:   "list",
		Short: "List the cartridges in the datastore.",
		RunE: func(cmd *cobra.Command, args []string) error {
			return listCartridges(cfg)
		},
	}

	return c
}

func buildSaveCartridgeCmd(cfg *config.Config) *cobra.Command {
	var (
		name        string
		size        string
		bore        float64
		cartridgeId uint
	)
	c := &cobra.Command{
		Use:   "save",
		Short: "Create or update a cartridge with the command line parameters.",
		RunE: func(cmd *cobra.Command, args []string) error {
			return saveCartridge(cfg, name, size, bore, cartridgeId)
		},
	}
	c.Flags().StringVarP(&name, "name", "", "", "A unique name for the cartridge.")
	c.Flags().StringVarP(&size, "size", "", "", "The size of the cartridge (7.62x39mm).")
	c.Flags().Float64VarP(&bore, "bore", "", 0.0, "The bore diameter, in inches.")
	c.Flags().UintVarP(&cartridgeId, "id", "", 0, "The id of the cartridge to update. Must be provide if you want to update an existing cartridge.")
	command.SetMandatoryFlags(c, "name", "size", "bore")

	return c
}

func buildDeleteCartridgeCmd(cfg *config.Config) *cobra.Command {
	var (
		cartridgeId uint
	)
	c := &cobra.Command{
		Use:   "delete",
		Short: "Delete the cartridge.",
		RunE: func(cmd *cobra.Command, args []string) error {
			return deleteCartridge(cfg, cartridgeId)
		},
	}
	c.Flags().UintVarP(&cartridgeId, "id", "", 0, "The id of the cartridge to update. Must be provide if you want to update an existing cartridge.")
	command.SetMandatoryFlags(c, "id")

	return c
}

func getApp(cfg *config.Config) (*mlrb.MyLittleRangeBook, error) {
	err := persistence.Bootstrap()
	if err != nil {
		return nil, err
	}

	return mlrb.New(cfg), nil
}

func listCartridges(cfg *config.Config) error {
	a, err := getApp(cfg)
	if err != nil {
		return err
	}

	cartridges, err := a.ListCartridges()
	for _, c := range cartridges {
		_, err := io.WriteString(a.Config.Out, fmt.Sprintf("%04d - %s\n", c.ID, c.String()))
		if err != nil {
			log.Fatal(err)
		}
	}

	return nil
}

func deleteCartridge(cfg *config.Config, cartridgeId uint) error {

	a, err := getApp(cfg)

	if err = a.DeleteCartridge(cartridgeId); err != nil {
		return err
	}

	return nil
}

func saveCartridge(cfg *config.Config, name string, size string, bore float64, cartridgeId uint) error {

	a, err := getApp(cfg)

	if err = a.SaveCartridgeToSqlLite(name, size, bore, cartridgeId); err != nil {
		return err
	}

	return nil
}
