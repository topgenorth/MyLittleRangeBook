package commands

import (
	"fmt"
	"github.com/spf13/cobra"
	_ "gocloud.dev/docstore/awsdynamodb"
	"opgenorth.net/labradar/pkg/model/cartridge"
	"sort"
)

func ListCartridgesCmd() *cobra.Command {

	cmd := &cobra.Command{
		Use:   "listcartridges",
		Short: "List the cartridges in Amplify",
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			return initializeConfig(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {
			// cartridges = getCartridgesFromAWS()

			cartridges, err := cartridge.Load()
			if err != nil {
				fmt.Println("Problem retrieving a list of cartridges. ", err)
				return
			}
			displayCartridgesToStdOut(cartridges)
		},
	}

	return cmd
}

func displayCartridgesToStdOut(cartridges []cartridge.Cartridge) {
	sort.Slice(cartridges[:], func(i, j int) bool {
		return cartridges[i].Name < cartridges[j].Name
	})

	for _, c := range cartridges {
		fmt.Println(c.ToString())
	}
}

