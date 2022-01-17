package cmd

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"opgenorth.net/mylittlerangebook/pkg/labradar/fs"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

// buildLabradarCommands will create the Cobra command for detailing with files from a Labradar.
func buildLabradarCommands(a *mlrb.MyLittleRangeBook) *cobra.Command {
	cmd := &cobra.Command{
		Use:              "labradar",
		Short:            "All the commands for dealing with Labradar files via the command line.",
		TraverseChildren: true,
	}

	cmd.AddCommand(buildReadLabradarCsvCmd(a))
	cmd.AddCommand(buildListFilesCmd(a))
	cmd.AddCommand(buildSubmitCsvFileCmd(a))
	cmd.AddCommand(buildDescribeSeriesCommand(a))

	return cmd
}

// buildSubmitCsvFileCmd will create the Cobra command to store/send the files to cloud storage.
func buildSubmitCsvFileCmd(a *mlrb.MyLittleRangeBook) *cobra.Command {

	var n int
	var i string
	cmd := &cobra.Command{
		Use:   "submit",
		Short: "Submit the CSV file.",
		Run: func(cmd *cobra.Command, args []string) {
			filename := fs.FilenameForSeries(i, n)
			err := a.SubmitLabradarCsv(filename)
			if err != nil {
				logrus.Error(err)
			} else {
				logrus.Info("Submitted the file " + fs.FilenameForSeries(i, n) + ".")
			}
		},
	}

	cmd.Flags().IntVarP(&n, "number", "n", 0, "The number of the OldDevice CSV file to read.")
	setMandatoryFlags(cmd, "number")

	cmd.Flags().StringVarP(&i, "labradar.inputDir", "", "", "The location of the input files.")

	return cmd
}

// buildReadLabradarCsvCmd will create the Cobra command to read a Labradar file and and display it to StdOut.
func buildReadLabradarCsvCmd(app *mlrb.MyLittleRangeBook) *cobra.Command {

	var i string
	var n int

	cmd := &cobra.Command{
		Use:   "read",
		Short: "Reads a Labradar CSV file and displays a summary to STDOUT.",
		PersistentPreRun: func(cmd *cobra.Command, args []string) {
			logrus.Debug("Pre-run")
		},
		Run: func(cmd *cobra.Command, args []string) {
			series, err := app.LoadLabradarCsv(i, n)
			if err != nil {
				logrus.Fatal(err)
				return
			}

			sw := labradar.StdOutSeriesWriter1{TemplateString: labradar.TMPL_SUMMARIZE_SERIES}
			if err := sw.Write(*series); err != nil {
				logrus.Fatal(err)
			}
		},
		PersistentPostRun: func(cmd *cobra.Command, args []string) {
			logrus.Debug("Post run.")
		},
	}

	cmd.Flags().IntVarP(&n, "number", "n", 0, "The number of the OldDevice CSV file to read.")
	cmd.Flags().StringVarP(&i, "labradar.inputDir", "", "", "The location of the input files.")

	return cmd
}

// buildListFilesCmd will create the Cobra command to list all the CSV files in an LBR directory.
func buildListFilesCmd(app *mlrb.MyLittleRangeBook) *cobra.Command {
	var inputDir string

	cmd := &cobra.Command{
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

	cmd.Flags().StringVarP(&inputDir, "labradar.inputDir", "", "", "The root directory of the labradar files (i.e. LBR).")
	setMandatoryFlags(cmd, "labradar.inputDir")
	return cmd
}
