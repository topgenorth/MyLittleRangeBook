package cmd

import (
	"github.com/spf13/cobra"
	"labreader/internal/logger"
)

/*
 Sample command lines:
	* [TO20231207] stats -s 5 -d C:\Users\tom.opgenorth\work\MyLittleRangeBook\data\LBR\Maverick-308\130gr-BarnesTSX-44_6gr-X4895
*/

var (
	directoryStr string
	seriesInt    int
	statsCmd     = &cobra.Command{
		Use:   "stats",
		Short: "Display some basic stats from the file in question.",
		Long:  `Display the average velocity, extreme spread, and standard deviation from the series.`,

		RunE: func(cmd *cobra.Command, args []string) error {
			//cfg, err := config.InitConfig()
			//if err != nil {
			//	return err
			//}

			logger.DefaultLogger().
				Debug().
				Str("data directory", directoryStr).
				Int("series", seriesInt).
				Msg("stats called")

			return nil
		},
	}
)

func init() {
	rootCmd.AddCommand(statsCmd)

	statsCmd.Flags().IntVarP(&seriesInt, "series-number", "s", 0, "The number of the series.")
	_ = statsCmd.MarkFlagRequired("series-number")

	statsCmd.Flags().StringVarP(&directoryStr, "dir", "d", "", "The path/directory for the series.")
	_ = statsCmd.MarkFlagRequired("dir")
	_ = statsCmd.MarkFlagDirname("dir")
}
