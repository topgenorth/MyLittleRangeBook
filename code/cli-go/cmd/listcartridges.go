package cmd

import (
	"github.com/spf13/cobra"
	_ "gocloud.dev/docstore/awsdynamodb"
	"opgenorth.net/mylittlerangebook/pkg/mylittlerangebook"
)

func ListCartridgesCmd() *cobra.Command {

	app := mylittlerangebook.New()

	cmd := &cobra.Command{
		Use:   "listcartridges",
		Short: "List the cartridges in Amplify",
		Run: func(cmd *cobra.Command, args []string) {
			app.ListCartridges()
		},
	}

	return cmd
}
