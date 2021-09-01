package commands

import (
	"github.com/spf13/cobra"
	_ "gocloud.dev/docstore/awsdynamodb"
	"log"
	"opgenorth.net/labradar/pkg/mylittlerangebook"
)

func ListCartridgesCmd() *cobra.Command {

	app, err := mylittlerangebook.New()
	if err != nil {
		log.Fatal(err)
	}

	cmd := &cobra.Command{
		Use:   "listcartridges",
		Short: "List the cartridges in Amplify",
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			return initializeConfig(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {
			app.ListCartridges()
		},
	}

	return cmd
}
