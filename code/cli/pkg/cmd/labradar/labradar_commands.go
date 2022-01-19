package labradar

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/cmd"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"opgenorth.net/mylittlerangebook/pkg/labradar/fs"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

// BuildLabradarCommands will create the Cobra command for detailing with files from a Labradar.
func BuildLabradarCommands(a *mlrb.MyLittleRangeBook) *cobra.Command {
	c := &cobra.Command{
		Use:              "labradar",
		Short:            "All the commands for dealing with Labradar files via the command line.",
		TraverseChildren: true,
	}

	c.AddCommand(buildReadLabradarCsvCmd(a))
	//c.AddCommand(buildListFilesCmd(a))
	//c.AddCommand(buildSubmitCsvFileCmd(a))
	//c.AddCommand(buildDescribeSeriesCommand(a))

	return c
}

// buildSubmitCsvFileCmd will create the Cobra command to store/send the files to cloud storage.
func buildSubmitCsvFileCmd(a *mlrb.MyLittleRangeBook) *cobra.Command {

	var n int
	var i string
	c := &cobra.Command{
		Use:   "submit",
		Short: "Submit the CSV file.",
		RunE: func(cmd *cobra.Command, args []string) error {
			filename := fs.FilenameForSeries(i, n)
			err := a.SubmitLabradarCsv(filename)
			if err != nil {
				return err
			}
			logrus.Info("Submitted the file " + fs.FilenameForSeries(i, n) + ".")
			return nil
		},
	}

	c.Flags().IntVarP(&n, "number", "n", 0, "The number of the OldDevice CSV file to read.")
	cmd.SetMandatoryFlags(c, "number")

	c.Flags().StringVarP(&i, "labradar.inputDir", "", "", "The location of the input files.")

	return c
}

// buildReadLabradarCsvCmd will create the Cobra command to read a Labradar file and and display it to StdOut.
func buildReadLabradarCsvCmd(app *mlrb.MyLittleRangeBook) *cobra.Command {
	var i string
	var n int

	c := &cobra.Command{
		Use:   "read",
		Short: "Reads a Labradar CSV file and displays a summary to STDOUT.",
		RunE: func(cmd *cobra.Command, args []string) error {
			series, err := app.LoadSeriesFromLabradar(i, n)
			if err != nil {
				return err
			}

			sw := labradar.StdOutSeriesWriter1{TemplateString: labradar.TMPL_SUMMARIZE_SERIES}
			if err := sw.Write(*series); err != nil {
				return fmt.Errorf("could not write the series %d to StdOut", n)
			}

			return nil
		},
	}

	c.Flags().IntVarP(&n, "number", "n", 0, "The number of the labradar series CSV file to read.")
	c.Flags().StringVarP(&i, "inputDir", "", "", "The location of the input files.")
	cmd.SetMandatoryFlags(c, "number", "inputDir")

	return c
}

// buildListFilesCmd will create the Cobra command to list all the CSV files in an LBR directory.
func buildListFilesCmd(app *mlrb.MyLittleRangeBook) *cobra.Command {
	var inputDir string

	c := &cobra.Command{
		Use:   "list",
		Short: "Will display a list of all the CSV files in the input directory.",
		Run: func(cmd *cobra.Command, args []string) {
			files, err := app.GetListOfLabradarFiles(inputDir)
			if err != nil {
				logrus.Panicf("Could not list files!  %v", err)
			}
			logrus.Infof("List %d files.", len(files))
		},
	}

	c.Flags().StringVarP(&inputDir, "labradar.inputDir", "", "", "The root directory of the labradar files (i.e. LBR).")
	cmd.SetMandatoryFlags(c, "labradar.inputDir")
	return c
}
