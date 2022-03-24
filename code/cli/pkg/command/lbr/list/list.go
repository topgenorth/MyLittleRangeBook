package list

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
)

type LabradarListCommandOptions struct {
}

// NewListLbrFilesCmd will create the Cobra command to list all the CSV files in an LBR directory.
func NewListLbrFilesCmd(cfg *config.Config) *cobra.Command {

	c := &cobra.Command{
		Use:   "list",
		Short: "Will display a list of all Labaradar series in the Labradar folder.",
		RunE: func(cmd *cobra.Command, args []string) error {
			var lbrDir = ""

			logrus.Debugf("List the series in %s.", lbrDir)
			series := labradar.WithDirectory(lbrDir).ListSeries()

			_, err := fmt.Fprintf(cfg.Out, "Series in %s:", lbrDir)
			if err != nil {
				return err
			}

			for _, s := range series {
				_, err := fmt.Fprintln(cfg.Out, fmt.Sprintf("  %v", s))
				if err != nil {
					return err
				}
			}

			return nil

		},
	}

	return c
}
