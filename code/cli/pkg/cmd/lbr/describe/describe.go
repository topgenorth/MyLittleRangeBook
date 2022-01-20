package describe

import (
	"github.com/pkg/errors"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/cmd"
	"opgenorth.net/mylittlerangebook/pkg/labradar/readme"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
	"path/filepath"
)

// BuildDescribeSeriesCommand will create the Cobra command to describe what the OldSeries is all about.
func BuildDescribeSeriesCommand(a *mlrb.MyLittleRangeBook) *cobra.Command {

	p := describeParameters{
		seriesNumber: 0,
		notes:        "",
		inputDir:     "",
		firearm:      "",
		cartridge:    "",
		bullet:       "",
		powder:       "",
		cbto:         0.0,
	}

	c := &cobra.Command{
		Use:   "describe",
		Short: "Describe the series.",
		Run: func(cmd *cobra.Command, args []string) {

			s, err := a.LoadSeriesFromLabradar(p.inputDir, p.seriesNumber)
			if err != nil {
				logrus.Fatal("Could not read the CSV file. %w", err)
			}
			//p.updateSeries(s)

			logrus.Warning("DescribeToStdOut was here.")
			//_ = a.DescribeToStdOut(s)

			file := filepath.Join(p.inputDir, "README.md")
			r, err := readme.Load(file, a.Config.Filesystem)
			if err != nil {
				logrus.Warnf("Will not append the series %d to the README file %s: %v", p.seriesNumber, file, err)
			} else {
				r.AppendSeries(*s, true)
				if err = readme.Save(*r, a.Config.Filesystem); err != nil {
					logrus.Errorf("Could not update the README %s: %v", file, errors.Unwrap(err))
					return
				}

				logrus.Infof("Updated the README file %s", file)
			}

		},
	}

	c.Flags().IntVarP(&p.seriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
	c.Flags().StringVarP(&p.cartridge, "cartridge", "", "", "The cartridge that was measured. e.g. 6.5 Grendel.")
	c.Flags().StringVarP(&p.firearm, "firearm", "", "", "The name of the firearm.")
	c.Flags().StringVarP(&p.inputDir, "lbr.inputDir", "", "", "The location of the input files.")
	cmd.SetMandatoryFlags(c, "cartridge", "firearm")

	c.Flags().StringVarP(&p.notes, "notes", "", "", "Some text to describe what this series is about.")
	c.Flags().StringVarP(&p.bullet, "bullet", "", "", "The bullet that was used. e.g. 123gr Hornady ELD Match.")
	c.Flags().StringVarP(&p.powder, "powder", "", "", "The weight and powder that was used. e.g. 29.1gr BL-C(2).")
	c.Flags().Float32VarP(&p.cbto, "cbto", "", 0, "The cartridge-to-base-ogive length, in inches.  e.g. 1.666")

	if err := cmd.InitializeCommand(c); err != nil {
		logrus.Errorf("There is a problem trying to initialize the commnad %s: %v", c.Name(), err)
	}

	return c
}

// describeParameters holds the values that are necessary to describe a give Labradar series.
type describeParameters struct {
	seriesNumber int
	notes        string
	inputDir     string
	firearm      string
	cartridge    string
	bullet       string
	powder       string
	cbto         float32
}

func (p *describeParameters) updateSeries(s series.LabradarSeries) {

	// TODO [TO20220118] Setup the mutators and apply them.

	//s.Notes = p.notes
	////s.Firearm.Name = p.firearm
	////s.LoadData.Cartridge = p.cartridge
	////s.LoadData.CBTO = p.cbto
	//
	//s.SetProjectile(p.bullet)
	//s.SetPowder(p.powder)

}
