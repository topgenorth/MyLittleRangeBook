package list

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/cmd"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
)

// BuildListFilesCmd will create the Cobra command to list all the CSV files in an LBR directory.
func BuildListFilesCmd(app *mlrb.MyLittleRangeBook) *cobra.Command {
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

	c.Flags().StringVarP(&inputDir, "lbr.inputDir", "", "", "The root directory of the lbr files (i.e. LBR).")
	cmd.SetMandatoryFlags(c, "lbr.inputDir")
	return c
}
