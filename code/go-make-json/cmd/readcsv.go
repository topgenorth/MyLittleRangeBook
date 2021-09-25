package cmd

//func ReadLabradarFileCmd() *cobra.Command {
//	app, err := mylittlerangebook.New()
//	if err != nil {
//		log.Fatal(err)
//	}
//
//	seriesNumber := 0
//	inputDirectory := ""
//	outputDirectory := ""
//	timezone := ""
//
//	convertCsvCmd := &cobra.Command{
//		Use:   "readcsv",
//		Short: "Reads a Device CSV file and converts it to JSON.",
//		Long:  `Currently this will read a CSV file and convert it to JSON.`,
//		PreRunE: func(cmd *cobra.Command, args []string) error {
//			return nil
//		},
//		Run: func(cmd *cobra.Command, args []string) {
//			jsonFile, err := app.ConvertLabradarCsvToJson("")
//			if err != nil {
//				log.Fatal(err)
//			}
//			log.Println(jsonFile)
//		},
//	}
//
//	// Define cobra flags, the default value has the lowest (least significant) precedence
//	convertCsvCmd.Flags().IntVarP(&seriesNumber, "number", "n", 0, "The number of the Device CSV file to read.")
//	convertCsvCmd.Flags().StringVarP(&inputDirectory, "inputDir", "i", "", "The location of the input files.")
//	convertCsvCmd.Flags().StringVarP(&outputDirectory, "outputDir", "o", "", "The location of the output files.")
//	convertCsvCmd.Flags().StringVarP(&timezone, "timezone", "", "", "The IANA timezone that the Labradar is in.")
//	return convertCsvCmd
//}
