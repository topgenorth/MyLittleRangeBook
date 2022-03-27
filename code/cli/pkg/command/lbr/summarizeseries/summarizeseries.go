package summarizeseries

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
)

//
//import (
//	"fmt"
//	"github.com/pkg/errors"
//	"github.com/sirupsen/logrus"
//	"github.com/spf13/cobra"
//	"opgenorth.net/mylittlerangebook/pkg/command"
//	"opgenorth.net/mylittlerangebook/pkg/config"
//	"opgenorth.net/mylittlerangebook/pkg/labradar"
//	"opgenorth.net/mylittlerangebook/pkg/mlrb"
//)
//
const (
	SeriesNumberFlag = "number"
)

// LabradarReadCommandOptions is all the parameters for labradar read.
type LabradarReadCommandOptions struct {
	Number            int
	LabradarDirectory labradar.DirectoryProviderFn
}

func (t LabradarReadCommandOptions) SeriesNumber() labradar.SeriesNumber {
	return labradar.SeriesNumber(t.Number)
}

//
//func (t LabradarReadCommandOptions) String() string {
//	return fmt.Sprintf("series %d in directory '%s'", t.SeriesNumber, t.LabradarDirectory())
//}
//
// NewCmdRead will create the Cobra command to read a Labradar file and display a summary to StdOut.
// lbrDir is my goofy way of trying to read an option that was bound by the parent command.  I can't figure out
// how to get the value of the lbr.LbrDirectoryFlagParam and bind it
func NewCmdRead(cfg *config.Config, lbrDirectoryProvider labradar.DirectoryProviderFn) *cobra.Command {

	opts := &LabradarReadCommandOptions{
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

func summarizeLabradarFile(cfg *config.Config, opts *LabradarReadCommandOptions) error {
	//a := mlrb.New(cfg)
	//
	//s, err := a.ReadLabradarSeries(opts.LabradarDirectory(), opts.SeriesNumber)
	//if err != nil {
	//	if errors.Is(err, labradar.SeriesNotFoundError{}) {
	//		logrus.WithError(err).Errorf("Could not find the series.")
	//	} else {
	//		logrus.WithError(err).Errorf("Could not read series %d from %s.", opts.SeriesNumber, opts.LabradarDirectory())
	//	}
	//	return err
	//}
	//
	//w := labradar.New(cfg.Out, labradar.SimplePlainText)
	//
	//if err := w.Write(*s); err != nil {
	//	logrus.WithError(err).Errorf("Could not summarize the series %s.", s.String())
	//	return err
	//}

	series, err := labradar.
		WithDirectory(opts.LabradarDirectory()).
		LoadSeries(opts.SeriesNumber())
	if err != nil {
		return err
	}

	logrus.Debugf("Loaded the series %s!", series.String())

	return nil
}
