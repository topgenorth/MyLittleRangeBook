package cmd

import (
	"github.com/pkg/errors"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/labradar/readme"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
	"path/filepath"
)

// buildDescribeSeriesCommand will create the Cobra command to describe what the OldSeries is all about.
func buildDescribeSeriesCommand(a *mlrb.MyLittleRangeBook) *cobra.Command {

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

	cmd := &cobra.Command{
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
			r, err := readme.Load(file, a.Config.FileSystem)
			if err != nil {
				logrus.Warnf("Will not append the series %d to the README file %s: %v", p.seriesNumber, file, err)
			} else {
				r.AppendSeries(*s, true)
				if err = readme.Save(*r, a.Config.FileSystem); err != nil {
					logrus.Errorf("Could not update the README %s: %v", file, errors.Unwrap(err))
					return
				}

				logrus.Infof("Updated the README file %s", file)
			}

		},
	}

	cmd.Flags().IntVarP(&p.seriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
	cmd.Flags().StringVarP(&p.cartridge, "cartridge", "", "", "The cartridge that was measured. e.g. 6.5 Grendel.")
	cmd.Flags().StringVarP(&p.firearm, "firearm", "", "", "The name of the firearm.")
	cmd.Flags().StringVarP(&p.inputDir, "labradar.inputDir", "", "", "The location of the input files.")
	setMandatoryFlags(cmd, "cartridge", "firearm")

	cmd.Flags().StringVarP(&p.notes, "notes", "", "", "Some text to describe what this series is about.")
	cmd.Flags().StringVarP(&p.bullet, "bullet", "", "", "The bullet that was used. e.g. 123gr Hornady ELD Match.")
	cmd.Flags().StringVarP(&p.powder, "powder", "", "", "The weight and powder that was used. e.g. 29.1gr BL-C(2).")
	cmd.Flags().Float32VarP(&p.cbto, "cbto", "", 0, "The cartridge-to-base-ogive length, in inches.  e.g. 1.666")

	if err := initializeCommand(cmd); err != nil {
		logrus.Errorf("There is a problem trying to initialize the commnad %s: %v", cmd.Name(), err)
	}

	return cmd
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