package describe

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

const (
	SeriesNumberParamName = "number"
	CartridgeParamName    = "cartridge"
	FirearmParamName      = "firearm"
	NotesParamName        = "notes"
	BulletParamName       = "bullet"
	PowderParamName       = "powder"
	CbtoParamName         = "cbto"
)

// describeSeriesOptions holds the values that are necessary to describe a give Labradar series.
type describeSeriesOptions struct {
	seriesNumber int
	notes        string
	firearm      string
	cartridge    string
	bullet       string
	powder       string
	cbto         float32
	labradarDir  func() string
}

// NewDescribeSeriesCmd will create the Cobra command to describe what the OldSeries is all about.
func NewDescribeSeriesCmd(cfg *config.Config, lbrDir func() string) *cobra.Command {

	p := describeSeriesOptions{
		seriesNumber: 0,
		notes:        "",
		firearm:      "",
		cartridge:    "",
		bullet:       "",
		powder:       "",
		cbto:         0.0,
		labradarDir:  lbrDir,
	}

	c := &cobra.Command{
		Use:   "describe",
		Short: "Describe the series.",
		RunE: func(cmd *cobra.Command, args []string) error {
			return describeSeries(cfg, p)
		},
	}

	c.Flags().IntVarP(&p.seriesNumber, SeriesNumberParamName, "n", 0, "The series number to read.")
	c.Flags().StringVarP(&p.cartridge, CartridgeParamName, "", "", "The cartridge that was measured. e.g. \"6.5 Grendel\".")
	c.Flags().StringVarP(&p.firearm, FirearmParamName, "", "", "The name of the firearm.")
	command.SetMandatoryFlags(c, CartridgeParamName, "firearm", "number")

	c.Flags().StringVarP(&p.notes, NotesParamName, "", "", "Some text to describe what this series is about.")
	c.Flags().StringVarP(&p.bullet, BulletParamName, "", "", "The bullet that was used. e.g. \"123gr Hornady ELD Match\".")
	c.Flags().StringVarP(&p.powder, PowderParamName, "", "", "The weight and powder that was used. e.g. \"29.1gr BL-C(2)\".")
	c.Flags().Float32VarP(&p.cbto, CbtoParamName, "", 0, "The cartridge-to-base-ogive length, in inches.  e.g. 1.666")

	return c
}

func describeSeries(cfg *config.Config, opts describeSeriesOptions) error {
	// TODO [TO20220123] clean up the repetition in the error handling.
	a, s, err := loadAndUpdateSeries(cfg, opts)
	if err != nil {
		return err
	}

	if err := displaySeries(cfg, opts); err != nil {
		return err
	}

	if err := updateReadme(a, s, opts); err != nil {
		return err
	}

	if err := saveDescription(a, s, opts); err != nil {
		return err
	}
	return nil
}

func displaySeries(cfg *config.Config, opts describeSeriesOptions) error {
	logrus.Info("TODO: display the updated series to stdout.")
	return nil
}

func loadAndUpdateSeries(cfg *config.Config, opts describeSeriesOptions) (*mlrb.MyLittleRangeBook, *series.LabradarSeries, error) {
	lbrDir := opts.labradarDir()
	a := mlrb.New(cfg)
	s, err := a.LoadSeriesFromLabradar(lbrDir, opts.seriesNumber)
	if err != nil {
		logrus.WithError(err).Errorf("Could not load series %d  from %s.", opts.seriesNumber, lbrDir)
		return a, nil, err
	}

	return a, s, nil
}

func updateReadme(a *mlrb.MyLittleRangeBook, series *series.LabradarSeries, opts describeSeriesOptions) error {

	logrus.Info("TODO: need to update the README.md")

	//file := filepath.Join(opts.labradarDir(), "README.md")
	//r, err := readme.Load(file, a.Filesystem)
	//if err != nil {
	//	logrus.WithError(err).Warnf("Will not append the series %d to the README file %s.", series.Number, file)
	//	return err
	//}
	//r.AppendSeries(*series, true)
	//if err = readme.Save(*r, a.Filesystem); err != nil {
	//	logrus.Errorf("Could not update the README %s: %v", file, errors.Unwrap(err))
	//	return err
	//}

	return nil
}

func saveDescription(a *mlrb.MyLittleRangeBook, labradarSeries *series.LabradarSeries, opts describeSeriesOptions) error {
	logrus.Info("TODO: save the series and description to it's own file.")
	return nil
}
