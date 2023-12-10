package lbr

import (
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
)

const (
	SeriesNumberFlag = "number"
)

// summarizeSeriesOptions is all the parameters for labradar summarize.
type summarizeSeriesOptions struct {
	Number            int
	LabradarDirectory labradar.DirectoryProviderFn
}

func (t summarizeSeriesOptions) SeriesNumber() labradar.SeriesNumber {
	return labradar.SeriesNumber(t.Number)
}

// NewSummarizeSeriesCmd will create the Cobra command to read a Labradar file and display a summary to StdOut.
// lbrDirectoryProvider is my goofy way of trying to read an option that was bound by the parent command.  I can't figure out
// how to get the value of the lbr.LbrDirectoryFlagParam and bind it
func NewSummarizeSeriesCmd(cfg *config.Config, lbrDirectoryProvider labradar.DirectoryProviderFn) *cobra.Command {

	opts := &summarizeSeriesOptions{
		LabradarDirectory: lbrDirectoryProvider,
	}

	readCmd := &cobra.Command{
		Use:   "summarize",
		Short: "Display a summary of the contents of a given Labradar series.",
		RunE: func(cmd *cobra.Command, args []string) error {
			return summarizeLabradarFile(cfg, opts)
		},
	}

	readCmd.Flags().IntVarP(&opts.Number, SeriesNumberFlag, "n", 0, "The number of the lbr series CSV file to read.")
	command.SetMandatoryFlags(readCmd, SeriesNumberFlag)

	return readCmd
}

func summarizeLabradarFile(cfg *config.Config, opts *summarizeSeriesOptions) error {

	series, err := labradar.
		WithDirectory(opts.LabradarDirectory()).
		LoadSeries(opts.SeriesNumber())
	if err != nil {
		return err
	}

	w := labradar.GetSummaryWriter(cfg.Out, labradar.DescriptivePlainText)
	if err := w.Write(*series); err != nil {
		return err
	}

	return nil
}
