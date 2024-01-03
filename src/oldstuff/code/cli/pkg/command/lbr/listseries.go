package lbr

import (
	"fmt"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
)

type LabradarListCommandOptions struct {
}

// NewListLbrFilesCmd will create the Cobra command to list all the CSV files in an LBR directory.
func NewListLbrFilesCmd(cfg *config.Config, lbrDirectoryProvider func() string) *cobra.Command {

	c := &cobra.Command{
		Use:   "list",
		Short: "Will display a list of all Labaradar series in the Labradar folder.",
		RunE: func(cmd *cobra.Command, args []string) error {
			lbrDir := lbrDirectoryProvider()
			series := labradar.WithDirectory(lbrDir).ListSeries()

			_, err := fmt.Fprintf(cfg.Out, "Series in %s:\n", lbrDir)
			if err != nil {
				return err
			}

			for _, s := range series {
				_, err := fmt.Fprintln(cfg.Out, fmt.Sprintf("  %s", s.String()))
				if err != nil {
					return err
				}
			}

			return nil

		},
	}

	return c
}
