package cmd

import (
	"github.com/spf13/cobra"
	"path/filepath"
	"strconv"
)

func ReadLabradarDirectoryCmd() *cobra.Command {

	inputDirectory := ""
	outputDirectory := ""
	timezone := ""

	cmd := &cobra.Command{
		Use:   "readdir",
		Short: "Reads a directory for Labradar files and converts them to JSON.",
		//PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
		//	// You can bind cobra and viper in a few locations, but PersistencePreRunE on the root command works well
		//	return initializeConfig(cmd)
		//},
		Run: func(cmd *cobra.Command, args []string) {
			//cfg := config.New()
			//cfg.InputDir = inputDirectory
			//cfg.OutputDir = outputDirectory
			//cfg.TimeZone, _ = time.LoadLocation(timezone)
			//
			//for idx, filename := range labradar.GetLabradarFilesInDir(inputDirectory) {
			//	seriesNumber := getSeriesNumberFrom(filename)
			//	err := readLabradarCsvAndConvertToJson(seriesNumber, cfg)
			//	if err != nil {
			//		fmt.Println(fmt.Sprintf("%d: Problems with the file %s.", idx, filename))
			//	} else {
			//		fmt.Println(fmt.Sprintf("%d: Converted the file  %s.", idx, filename))
			//	}
			//}
		},
	}

	cmd.Flags().StringVarP(&inputDirectory, "inputDir", "i", "", "The location of the input files.")
	cmd.Flags().StringVarP(&outputDirectory, "outputDir", "o", "", "The location of the output files.")
	cmd.Flags().StringVarP(&timezone, "timezone", "", "", "The IANA timezone that the Labradar is in.")

	return cmd
}

func getSeriesNumberFrom(path string) int {
	_, file := filepath.Split(path)

	seriesStr := file[2:6]
	i, err := strconv.Atoi(seriesStr)
	if err != nil {
		panic(err)
	}

	return i
}
