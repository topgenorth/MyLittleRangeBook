package read

import (
	"fmt"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/cmd"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

// BuildReadLabradarCsvCmd will create the Cobra command to read a Labradar file and and display it to StdOut.
func BuildReadLabradarCsvCmd(app *mlrb.MyLittleRangeBook) *cobra.Command {
	var i string
	var n int

	c := &cobra.Command{
		Use:   "read",
		Short: "Reads a Labradar CSV file and displays a summary to STDOUT.",
		RunE: func(cmd *cobra.Command, args []string) error {
			series, err := app.LoadSeriesFromLabradar(i, n)
			if err != nil {
				return err
			}

			sw := labradar.StdOutSeriesWriter1{TemplateString: labradar.TMPL_SUMMARIZE_SERIES}
			if err := sw.Write(*series); err != nil {
				return fmt.Errorf("could not write the series %d to StdOut", n)
			}

			return nil
		},
	}

	c.Flags().IntVarP(&n, "number", "n", 0, "The number of the lbr series CSV file to read.")
	c.Flags().StringVarP(&i, "inputDir", "", "", "The location of the input files.")
	cmd.SetMandatoryFlags(c, "number", "inputDir")

	return c
}
