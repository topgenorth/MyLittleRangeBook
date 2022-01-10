package cmd

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"opgenorth.net/mylittlerangebook/pkg/labradar/io"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

func buildLabradarCommands(a *mlrb.MyLittleRangeBook) *cobra.Command {
	cmd := &cobra.Command{
		Use:   "labradar",
		Short: "All the commands for dealing with Labradar files via the command line.",
	}

	cmd.AddCommand(buildReadLabradarCsvCmd(a))
	cmd.AddCommand(buildListLabradarCsvFilesCmd(a))
	cmd.AddCommand(buildSubmitCsvFileCmd(a))
	cmd.AddCommand(buildDescribeSeriesCommand(a))
	return cmd
}

func buildSubmitCsvFileCmd(a *mlrb.MyLittleRangeBook) *cobra.Command {

	var seriesNumber int
	var inputDir string
	cmd := &cobra.Command{
		Use:   "submit",
		Short: "Submit the CSV file.",
		Run: func(cmd *cobra.Command, args []string) {
			filename := labradar.FilenameForSeries(inputDir, seriesNumber)
			err := a.SubmitLabradarCsv(filename)
			if err != nil {
				logrus.Error(err)
			} else {
				logrus.Info("Submitted the file " + labradar.FilenameForSeries(inputDir, seriesNumber) + ".")
			}
		},
	}

	cmd.Flags().IntVarP(&seriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
	setMandatoryFlags(cmd, "number")

	cmd.Flags().StringVarP(&inputDir, "labradar.inputDir", "", "", "The location of the input files.")

	return cmd
}

func buildReadLabradarCsvCmd(app *mlrb.MyLittleRangeBook) *cobra.Command {

	var inputDir string
	var seriesNumber int

	cmd := &cobra.Command{
		Use:   "read",
		Short: "Reads a Labradar CSV file and displays a summary to STDOUT.",
		Run: func(cmd *cobra.Command, args []string) {
			series, err := app.LoadLabradarCsv(inputDir, seriesNumber)
			if err != nil {
				logrus.Fatal(err)
				return
			}

			sw := io.StdOutSeriesWriter1{TemplateString: io.TMPL_SUMMARIZE_SERIES}
			if err := sw.Write(*series); err != nil {
				logrus.Fatal(err)
			}
		},
	}

	cmd.Flags().IntVarP(&seriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
	setMandatoryFlags(cmd, "number")

	cmd.Flags().StringVarP(&inputDir, "labradar.inputDir", "", "", "The location of the input files.")

	return cmd
}

func buildListLabradarCsvFilesCmd(app *mlrb.MyLittleRangeBook) *cobra.Command {
	var inputDir string

	cmd := &cobra.Command{
		Use:   "list",
		Short: "Will display a list of all the CSV files in the input directory.",
		Run: func(cmd *cobra.Command, args []string) {
			_, err := app.ListLabradarCsvFiles(inputDir)
			if err != nil {
				logrus.Fatal(err)
			}
		},
	}

	cmd.Flags().StringVarP(&inputDir, "labradar.inputDir", "", "", "The root directory of the labradar files (i.e. LBR).")

	return cmd
}

// Sets the specified flags as mandatory.  This is a helper method to reduce some of the repetitiveness with
// setting mandatory flags. If there is an error setting the mandatory flag, then a warning would be logged.
func setMandatoryFlags(cmd *cobra.Command, flagnames ...string) {
	type f struct {
		flagName string
		success  bool
		c        *cobra.Command
	}

	flags := make([]f, len(flagnames))
	for _, n := range flagnames {
		err := cmd.MarkFlagRequired(n)
		flags = append(flags, f{flagName: n, success: err == nil, c: cmd})
		if err != nil {
			logrus.Warnf("Could not make the flag %s mandatory: %v.", n, err.Error())
		}
	}

	// [TO20220110] Maybe do something like a warning if a flag could not be set?
}
