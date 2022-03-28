package lbr

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/command"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
	"opgenorth.net/mylittlerangebook/pkg/readme"
	"opgenorth.net/mylittlerangebook/pkg/util"
	"path/filepath"
	"strconv"
	"strings"
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

type DescribeSeriesOptions struct {
	seriesNumber int
	labradarDir  labradar.DirectoryProviderFn

	projectile string
	cartridge  string
	cbto       float32
	firearm    string
	notes      string
	powder     string
}

func (opts *DescribeSeriesOptions) SeriesNumber() labradar.SeriesNumber {
	return labradar.SeriesNumber(opts.seriesNumber)
}

func (opts *DescribeSeriesOptions) LabradarDir() string {
	return opts.labradarDir()
}

func (opts *DescribeSeriesOptions) Projectile() *labradar.Projectile {
	return parseProjectileString(opts.projectile)
}

func (opts *DescribeSeriesOptions) Cartridge() string {
	return opts.cartridge
}

func (opts *DescribeSeriesOptions) Cbto() float32 {
	return opts.cbto
}

func (opts *DescribeSeriesOptions) Firearm() string {
	return opts.firearm
}

func (opts *DescribeSeriesOptions) Notes() string {
	return opts.notes
}

func (opts *DescribeSeriesOptions) PowderCharge() *labradar.PowderCharge {
	return parsePowderString(opts.powder)
}

// NewDescribeSeriesCmd will create the Cobra command to describe what the OldSeries is all about.
// lbrDirectoryProvider is my goofy way of trying to read an option that was bound by the parent command.  I can't figure out
// how to get the value of the lbr.LbrDirectoryFlagParam and bind it
func NewDescribeSeriesCmd(cfg *config.Config, lbrDirectoryProvider labradar.DirectoryProviderFn) *cobra.Command {

	p := DescribeSeriesOptions{
		seriesNumber: 0,
		notes:        "",
		firearm:      "",
		cartridge:    "",
		projectile:   "",
		powder:       "",
		cbto:         0.0,
		labradarDir:  lbrDirectoryProvider,
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

func describeSeries(cfg *config.Config, opts DescribeSeriesOptions) error {

	number := opts.SeriesNumber()
	s, err := labradar.WithDirectory(opts.LabradarDir()).LoadSeries(number)
	if err != nil {
		return err
	}

	opts.updateSeries(s)
	logrus.Tracef("Retrieved %s and updated it.", number.String())

	//if err := a.SaveLabradarSeriesToJson(opts.labradarDir(), s); err != nil {
	//	return err
	//}
	//
	//if err := a.DisplaySeries(*s, true); err != nil {
	//	return err
	//}
	//
	//if err := updateReadme(a, s, opts); err != nil {
	//	return err
	//}

	return fmt.Errorf("Have to finish describeSeries!")
}

func updateReadme(a *mlrb.MyLittleRangeBook, series *labradar.Series, opts DescribeSeriesOptions) error {

	// TODO [TO20220123] How do we reconcile difference between an existing JSON file and the CSV file?

	file := filepath.Join(opts.labradarDir(), "README.md")

	logrus.Tracef("Updating the README.md (%s).", file)

	r, err := readme.Load(file, a.Filesystem)
	if err != nil {
		return err
	}
	r.AppendSeries(series, false)

	if err = readme.Save(*r, a.Filesystem); err != nil {
		return err
	}

	return nil
}

func (opts *DescribeSeriesOptions) updateSeries(s *labradar.Series) {

	// TODO [TO20220328] Unit tests
	mutators := make([]labradar.SeriesMutatorFn, 0)

	if len(opts.cartridge) > 0 {
		mutators = append(mutators, labradar.WithCartridge(opts.Cartridge()))
	}

	if len(opts.projectile) > 0 {
		mutators = append(mutators, labradar.WithProjecticle(opts.Projectile().Name, opts.Projectile().Weight))
	}

	if opts.cbto > 0 {
		mutators = append(mutators, func(s *labradar.Series) { s.LoadData.CBTO = opts.Cbto() })
	}

	if len(opts.firearm) > 0 {
		mutators = append(mutators, labradar.WithFirearm(opts.Firearm()))
	}

	if len(opts.notes) > 0 {
		mutators = append(mutators, labradar.WithNotes(opts.Notes()))
	}

	if len(opts.powder) > 0 {
		mutators = append(mutators, labradar.WithPowder(opts.PowderCharge().Name, opts.PowderCharge().Amount))
	}

	s.Update(mutators...)
}

func parsePowderString(powder string) *labradar.PowderCharge {
	parts := util.RemoveEmptyStrings(strings.Split(powder, " "))
	if len(parts) < 1 {
		return &labradar.PowderCharge{Name: "Unknown", Amount: 0.0}
	}

	p := &labradar.PowderCharge{
		Name:   parseNameOfProjectileFromString(strings.Join(parts[1:], " ")),
		Amount: parseAmountFromPowderString(parts[0]),
	}
	return p
}

func parseProjectileString(projectile string) *labradar.Projectile {
	parts := util.RemoveEmptyStrings(strings.Split(projectile, " "))

	if len(parts) < 1 {
		return &labradar.Projectile{Name: "Unknown", Weight: 0, BC: nil}
	}

	p := &labradar.Projectile{
		Name:   parseNameOfProjectileFromString(strings.Join(parts[1:], " ")),
		Weight: parseWeightFromProjectileString(parts[0]),
		BC:     nil, // [TO20220106] We don't worry about BC right now.
	}

	return p
}

func parseNameOfProjectileFromString(name string) string {

	replacer := strings.NewReplacer(
		"grains", "",
		"grain", "",
		"gr.", "",
		"gr", "",
	)
	return strings.TrimSpace(replacer.Replace(name))
}

func parseAmountFromPowderString(amount string) float32 {

	replacer := strings.NewReplacer(
		"grains", "",
		"grain", "",
		"gr.", "",
		"gr", "",
	)

	str := strings.TrimSpace(replacer.Replace(amount))

	w, err := strconv.ParseFloat(str, 32)
	if err != nil {
		return 0.0
	}

	return float32(w)
}

func parseWeightFromProjectileString(weight string) int {

	replacer := strings.NewReplacer(
		"grains", "",
		"grain", "",
		"gr.", "",
		"gr", "",
	)

	str := strings.TrimSpace(replacer.Replace(weight))

	w, err := strconv.ParseFloat(str, 32)
	if err != nil {
		return 0
	}

	return int(w)
}
