package read

import (
	"fmt"
	"github.com/pkg/errors"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series/summarywriter"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

const (
	SeriesNumberFlag = "number"
)

type LabradarDirectoryProvider = func() string // TODO [TO20220122] Duplication with list.NewListLbrFilesCmd

// LabradarReadCommandOptions is all the parameters for labradar read.
type LabradarReadCommandOptions struct {
	SeriesNumber      int
	LabradarDirectory LabradarDirectoryProvider
}

func (t LabradarReadCommandOptions) String() string {
	return fmt.Sprintf("series %d in directory '%s'", t.SeriesNumber, t.LabradarDirectory())
}

// NewCmdRead will create the Cobra command to read a Labradar file and display a summary to StdOut.
// lbrDir is my goofy way of trying to read an option that was bound by the parent command.  I can't figure out
// how to get the value of the lbr.LbrDirectoryFlagParam and bind it
func NewCmdRead(cfg *config.Config, lbrDir LabradarDirectoryProvider) *cobra.Command {

	opts := &LabradarReadCommandOptions{
		LabradarDirectory: lbrDir,
	}

	readCmd := &cobra.Command{
		Use:   "read",
		Short: "Reads a Labradar CSV file and displays a summary.",
		RunE: func(cmd *cobra.Command, args []string) error {
			return summarizeLabradarFile(cfg, opts)
		},
	}

	readCmd.Flags().IntVarP(&opts.SeriesNumber, SeriesNumberFlag, "n", 0, "The number of the lbr series CSV file to read.")
	command.SetMandatoryFlags(readCmd, SeriesNumberFlag)

	return readCmd
}

func summarizeLabradarFile(cfg *config.Config, opts *LabradarReadCommandOptions) error {
	a := mlrb.New(cfg)

	s, err := a.ReadLabradarSeries(opts.LabradarDirectory(), opts.SeriesNumber)
	if err != nil {
		if errors.Is(err, series.NotFoundError{}) {
			logrus.WithError(err).Errorf("Could not find the series.")
		} else {
			logrus.WithError(err).Errorf("Could not read series %d from %s.", opts.SeriesNumber, opts.LabradarDirectory())
		}
		return err
	}

	w := summarywriter.New(cfg.Out, summarywriter.SimplePlainText)

	if err := w.Write(*s); err != nil {
		logrus.WithError(err).Errorf("Could not summarize the series %s.", s.String())
		return err
	}

	return nil
}
