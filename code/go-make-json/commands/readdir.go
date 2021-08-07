package commands

import (
	"fmt"
	"github.com/spf13/cobra"
)

func ReadLabradarDirectoryCmd() *cobra.Command {

	cmd := &cobra.Command{
		Use:   "readcsv",
		Short: "Reads a Device CSV file and converts it to JSON.",
		Long:  `Currently this will read a CSV file and convert it to JSON.`,
		Args:  cobra.MinimumNArgs(1),
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			// You can bind cobra and viper in a few locations, but PersistencePreRunE on the root command works well
			return initializeConfig(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {
			fmt.Println("read a directory of Labradar CSV files")
			/*			ls := labradar.NewSeries(seriesNumber, cfg)
						err := labradar.LoadLabradarSeriesFromCsv(ls, cfg)
						if err != nil {
							return
						}

						err2 := labradar.SaveLabradarSeriesToJson(ls, cfg)
						if err2 != nil {
							fmt.Println(err2)
						}
			*/
		},
	}

	return cmd
}
