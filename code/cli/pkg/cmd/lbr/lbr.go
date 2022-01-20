// Package lbr holds the code to build the cobra.Command structs for interacting with Labradar CSV files.
package lbr

import (
	"github.com/spf13/cobra"
	cmd2 "opgenorth.net/mylittlerangebook/pkg/cmd/lbr/read"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

// NewCmd will create the Cobra command for detailing with files from a Labradar.
func NewCmd(a *mlrb.MyLittleRangeBook) *cobra.Command {
	c := &cobra.Command{
		Use:   "lbr <command>",
		Short: "Manage Labradar data files.",
		Long:  "View, upload, and describe the Labradar series.",
	}

	c.AddCommand(cmd2.BuildReadLabradarCsvCmd(a))
	//c.AddCommand(BuildListFilesCmd(a))
	//c.AddCommand(BuildSubmitCsvFileCmd(a))
	//c.AddCommand(BuildDescribeSeriesCommand(a))

	return c
}
