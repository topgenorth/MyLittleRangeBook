// Package lbr holds the code to build the cobra.Command structs for interacting with Labradar CSV files.
package lbr

import (
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	lbrRead "opgenorth.net/mylittlerangebook/pkg/command/lbr/read"
	"opgenorth.net/mylittlerangebook/pkg/config"
)

const (
	LbrDirectoryFlagParam = "lbr_dir"
)

type labradarOptions struct {
	LabradarDirectory string
}

// NewLabradarCmd will create the Cobra command for detailing with files from a Labradar.
func NewLabradarCmd(cfg *config.Config) *cobra.Command {
	opts := labradarOptions{}

	lbrCmd := &cobra.Command{
		Use:   "labradar <command>",
		Short: "Manage Labradar data files.",
		Long:  "View, upload, and describe the Labradar series.",
	}

	lbrCmd.PersistentFlags().StringVarP(&opts.LabradarDirectory, LbrDirectoryFlagParam, "d", "", "The directory holding the LBR files.")
	command.SetMandatoryFlags(lbrCmd, LbrDirectoryFlagParam)

	lbrCmd.AddCommand(lbrRead.NewCmdRead(cfg))
	//c.AddCommand(BuildListFilesCmd(a))
	//c.AddCommand(BuildSubmitCsvFileCmd(a))
	//c.AddCommand(BuildDescribeSeriesCommand(a))

	return lbrCmd
}
