package read

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

const (
	SeriesNumberFlag = "number"
)

// LabradarReadCmdOptions is all the parameters for labradar read.
type LabradarReadCmdOptions struct {
	SeriesNumber int
	LbrDirectory string
}

func (t LabradarReadCmdOptions) String() string {
	return fmt.Sprintf("series %d in directory '%s'", t.SeriesNumber, t.LbrDirectory)
}

// NewCmdRead will create the Cobra command to read a Labradar file and display a summary to StdOut.
func NewCmdRead(cfg *config.Config) *cobra.Command {

	opts := &LabradarReadCmdOptions{}

	readCmd := &cobra.Command{
		Use:   "read",
		Short: "Reads a Labradar CSV file and displays a summary.",
		RunE: func(cmd *cobra.Command, args []string) error {
			return readLabradarCsvFile(opts)
		},
	}

	// TODO [TO20220121] Maybe the default should be the current directory (working directory) with LBR appended?
	readCmd.Flags().IntVarP(&opts.SeriesNumber, SeriesNumberFlag, "n", 0, "The number of the lbr series CSV file to read.")
	command.SetMandatoryFlags(readCmd, SeriesNumberFlag)

	return readCmd
}

func readLabradarCsvFile(opts *LabradarReadCmdOptions) error {
	a := mlrb.New()

	s, err := a.LoadSeriesFromLabradar(opts.LbrDirectory, opts.SeriesNumber)
	if err != nil {
		return err
	}

	logrus.Tracef("Loaded '%s", s)

	return nil
}
