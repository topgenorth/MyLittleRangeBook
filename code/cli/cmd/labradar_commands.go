package cmd

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
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

func buildDescribeSeriesCommand(a *mlrb.MyLittleRangeBook) *cobra.Command {
	var seriesNumber int
	var notes string
	var inputDir string

	cmd := &cobra.Command{
		Use:   "describe",
		Short: "Describe the series.",
		Run: func(cmd *cobra.Command, args []string) {
			s, err := a.ReadLabradarCsv(inputDir, seriesNumber)
			if err != nil {
				logrus.Fatal("Could not read the CSV file. %v", err)
			}
			s.Notes = notes
		},
	}

	cmd.Flags().IntVarP(&seriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
	cmd.Flags().StringVarP(&notes, "text", "t", "", "Some text to describe what this series is about.")
	cmd.Flags().StringVarP(&inputDir, "labradar.inputDir", "i", "", "The location of the input files.")

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
	cmd.Flags().StringVarP(&inputDir, "labradar.inputDir", "i", "", "The location of the input files.")

	return cmd
}

func buildReadLabradarCsvCmd(app *mlrb.MyLittleRangeBook) *cobra.Command {

	var inputDir string
	var seriesNumber int

	cmd := &cobra.Command{
		Use:   "read",
		Short: "Reads a Labradar CSV file and displays a summary to STDOUT.",
		Run: func(cmd *cobra.Command, args []string) {
			series, err := app.ReadLabradarCsv(inputDir, seriesNumber)
			if err != nil {
				logrus.Fatal(err)
			}
			series.Print()
		},
	}

	cmd.Flags().IntVarP(&seriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
	cmd.Flags().StringVarP(&inputDir, "labradar.inputDir", "i", "", "The location of the input files.")

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

	cmd.Flags().StringVarP(&inputDir, "labradar.inputDir", "i", "", "The root directory of the labrada files.")

	return cmd
}
