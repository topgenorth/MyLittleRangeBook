package submit

//
//import (
//	"github.com/sirupsen/logrus"
//	"github.com/spf13/cobra"
//	"opgenorth.net/mylittlerangebook/pkg/command"
//	"opgenorth.net/mylittlerangebook/pkg/labradar"
//	"opgenorth.net/mylittlerangebook/pkg/mlrb"
//)
//
//// BuildSubmitCsvFileCmd will create the Cobra command to store/send the files to cloud storage.
//func BuildSubmitCsvFileCmd(a *mlrb.MyLittleRangeBook) *cobra.Command {
//
//	var n int
//	var i string
//	c := &cobra.Command{
//		Use:   "submit",
//		Short: "Submit the CSV file.",
//		RunE: func(cmd *cobra.Command, args []string) error {
//			filename := labradar.FilenameForSeries(i, n)
//			err := a.SubmitLabradarCsv(filename)
//			if err != nil {
//				return err
//			}
//			logrus.Info("Submitted the file " + labradar.FilenameForSeries(i, n) + ".")
//			return nil
//		},
//	}
//
//	c.Flags().IntVarP(&n, "number", "n", 0, "The number of the OldDevice CSV file to read.")
//	command.SetMandatoryFlags(c, "number")
//
//	c.Flags().StringVarP(&i, "lbr.inputDir", "", "", "The location of the input files.")
//
//	return c
//}
