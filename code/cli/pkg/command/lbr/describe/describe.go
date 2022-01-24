package describe

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series/jsonwriter"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
	"path/filepath"
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

// NewDescribeSeriesCmd will create the Cobra command to describe what the OldSeries is all about.
func NewDescribeSeriesCmd(cfg *config.Config, lbrDir func() string) *cobra.Command {

	p := describeSeriesOptions{
		seriesNumber: 0,
		notes:        "",
		firearm:      "",
		cartridge:    "",
		projectile:   "",
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
	command.SetMandatoryFlags(c, CartridgeParamName, FirearmParamName, "number")

	c.Flags().StringVarP(&p.notes, NotesParamName, "", "", "Some text to describe what this series is about.")
	c.Flags().StringVarP(&p.projectile, BulletParamName, "", "", "The bullet that was used. e.g. \"123gr Hornady ELD Match\".")
	c.Flags().StringVarP(&p.powder, PowderParamName, "", "", "The weight and powder that was used. e.g. \"29.1gr BL-C(2)\".")
	c.Flags().Float32VarP(&p.cbto, CbtoParamName, "", 0, "The cartridge-to-base-ogive length, in inches.  e.g. 1.666")

	return c
}

func describeSeries(cfg *config.Config, opts describeSeriesOptions) error {
	// TODO [TO20220123] clean up the repetition in the error handling.

	a := mlrb.New(cfg)
	s, err := loadSeries(a, opts)
	if err != nil {
		return err
	}
	opts.updateSeries(s)

	if err := saveSeriesToJsonFile(a, s, opts.labradarDir()); err != nil {
		return err
	}

	if err := displaySeries(); err != nil {
		return err
	}

	if err := updateReadme(a, s, opts); err != nil {
		return err
	}

	return nil
}

func loadSeries(a *mlrb.MyLittleRangeBook, opts describeSeriesOptions) (*series.LabradarSeries, error) {
	s, err := a.LoadSeriesFromLabradar(opts.labradarDir(), opts.seriesNumber)
	if err != nil {
		return nil, err
	}

	return s, err
}

func displaySeries() error {
	logrus.Info("TODO: display the updated series to stdout.")
	return nil
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

// saveSeriesToJsonFile will save the JSON file to disk.  It will delete any existing file.
func saveSeriesToJsonFile(a *mlrb.MyLittleRangeBook, s *series.LabradarSeries, dir string) error {

	seriesname := fmt.Sprintf("SR%s", s.SeriesName())
	filename := filepath.Join(dir, seriesname, seriesname+".json")

	exists, err := a.Filesystem.Exists(filename)
	if err != nil {
		return err
	}
	if exists {
		if err = a.Filesystem.Remove(filename); err != nil {
			return err
		}
		logrus.Debugf("Deleting the file `%s`.", filename)
	}

	w := jsonwriter.New(a.Filesystem, func() string { return filename })
	if err := w.Write(*s); err != nil {
		return err
	}

	return nil
}

// describeSeriesOptions holds the values that are necessary to describe a given series.LabradarSeries.
type describeSeriesOptions struct {
	seriesNumber int
	labradarDir  func() string

	projectile string
	cartridge  string
	cbto       float32
	firearm    string
	notes      string
	powder     string
}

func (opts *describeSeriesOptions) updateSeries(s *series.LabradarSeries) {

	mutators := make([]series.LabradarSeriesMutatorFunc, 0)

	if len(opts.cartridge) > 0 {
		mutators = append(mutators, series.WithCartridge(opts.cartridge))
	}

	if len(opts.projectile) > 0 {

		p := parseProjectileString(opts.projectile)
		mutators = append(mutators, series.WithProjecticle(p.Name, p.Weight))
	}

	if len(opts.cartridge) > 0 {
		mutators = append(mutators, series.WithCartridge(opts.cartridge))
	}

	if opts.cbto > 0 {
		f := func(s *series.LabradarSeries) { s.LoadData.CBTO = opts.cbto }
		mutators = append(mutators, f)
	}

	if len(opts.firearm) > 0 {
		mutators = append(mutators, series.WithFirearm(opts.firearm))
	}

	if len(opts.notes) > 0 {
		mutators = append(mutators, series.WithNotes(opts.notes))
	}

	if len(opts.powder) > 0 {
		pc := parsePowderString(opts.powder)
		mutators = append(mutators, series.WithPowder(pc.Name, pc.Amount))
	}

	s.Update(mutators...)
}
