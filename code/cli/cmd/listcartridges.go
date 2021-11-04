package cmd

import (
	"github.com/spf13/cobra"
	_ "gocloud.dev/docstore/awsdynamodb"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

func ListCartridgesCmd() *cobra.Command {

	app := mlrb.New()

	cmd := &cobra.Command{
		Use:   "listcartridges",
		Short: "List the cartridges in Amplify",
		Run: func(cmd *cobra.Command, args []string) {
			app.ListCartridges()
		},
	}

	return cmd
}
