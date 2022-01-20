package read

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/cmd"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

type ReadOptions struct {
	SeriesNumber int
	LbrDirectory string
}

func (t ReadOptions) String() string {
	return fmt.Sprintf("series %d in directory '%s'", t.SeriesNumber, t.LbrDirectory)
}

// NewCmdRead will create the Cobra command to read a Labradar file and display a summary to StdOut.
func NewCmdRead() *cobra.Command {

	opts := &ReadOptions{}

	c := &cobra.Command{
		Use:   "read",
		Short: "Reads a Labradar CSV file and displays a summary.",
		RunE: func(cmd *cobra.Command, args []string) error {

			a := mlrb.New()

			s, err := a.LoadSeriesFromLabradar(opts.LbrDirectory, opts.SeriesNumber)
			if err != nil {
				return err
			}

			logrus.Tracef("Loaded '%s", s)

			return nil
		},
	}
	c.Flags().IntVarP(&opts.SeriesNumber, "number", "n", 0, "The number of the lbr series CSV file to read.")
	c.Flags().StringVarP(&opts.LbrDirectory, "inputDir", "d", "", "The path to the LBR folder of the Labradar.")
	cmd.SetMandatoryFlags(c, "number")

	return c
}
