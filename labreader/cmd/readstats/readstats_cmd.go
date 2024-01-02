package readstats

import (
	"bufio"
	"fmt"
	"github.com/pkg/errors"
	"github.com/rs/zerolog"
	"github.com/spf13/afero"
	"github.com/spf13/cobra"
	"github.com/spf13/viper"
	"labreader/internal/logger"
	"labreader/pkg/labradar"
	"labreader/pkg/series"
	"os"
	"path/filepath"
)

/*
 Sample command lines:
	* [TO20231207] stats -s 12 -d C:\Users\tom.opgenorth\Dropbox\Firearms\MyLogs\Renegade\6x45\6x45-95grNosler-IMR8208XBR
*/

var (
	afs *afero.Afero
	l   zerolog.Logger
)

type commandLineValues struct {
	SeriesNumber  int    `mapstructure:"series-number"`
	Directory     string `mapstructure:"dir"`
	CatalogFolder string `mapstructure:"LBR_CATALOG_FOLDER"`
}

func (c commandLineValues) Filename() string {
	filename := fmt.Sprintf("series%d.csv", c.SeriesNumber)
	path := filepath.Join(c.Directory, filename)
	return path
}

func NewReadStatsCommand() *cobra.Command {
	statsCmd := &cobra.Command{
		Use:   "stats",
		Short: "Display some basic stats from the file in question.",
		Long:  `Display the average velocity, extreme spread, and standard deviation from the series.`,
		RunE:  readStatsFromCsv,
	}
	return configCliFlags(statsCmd)
}

func configCliFlags(readStatsCmd *cobra.Command) *cobra.Command {
	readStatsCmd.PersistentFlags().StringP("catalog-folder", "", "", "The path to the folder that holds the catalog files.")
	_ = readStatsCmd.MarkFlagRequired("catalog-folder")
	_ = viper.BindEnv("LBR_CATALOG_FOLDER")

	readStatsCmd.PersistentFlags().Int32P("series-number", "s", -1, "The number of the series.")
	_ = viper.BindPFlag("series-number", readStatsCmd.PersistentFlags().Lookup("series-number"))
	_ = readStatsCmd.MarkFlagRequired("series-number")

	readStatsCmd.PersistentFlags().StringP("dir", "d", "", "The path/directory for the series.")
	_ = viper.BindPFlag("dir", readStatsCmd.PersistentFlags().Lookup("dir"))
	_ = readStatsCmd.MarkFlagRequired("dir")
	_ = readStatsCmd.MarkFlagDirname("dir")
	return readStatsCmd
}

func readStatsFromCsv(cmd *cobra.Command, args []string) error {
	cliValues := &commandLineValues{}
	if err := viper.Unmarshal(cliValues); err != nil {
		return errors.Wrap(err, "could not bind the command line values")
	}

	if _, err := afs.Stat(cliValues.Filename()); errors.Is(err, os.ErrNotExist) {
		return errors.Wrapf(err, "the file %s does not exist", cliValues)
	}

	lines, err := readCsv(cliValues.Filename(), afs)
	if err != nil {
		return errors.Wrapf(err, "could not read the contents of the file %s", cliValues.Filename())
	}

	csvFile := labradar.CsvFileContents{
		Err:   nil,
		Lines: lines,
	}

	s, err := series.New2(withVelocities(csvFile),
		withSeriesNumber(csvFile))

	l.Debug().
		Int("number_of_shots", s.Velocities.CountOfShots()).
		Int("average_velocity", s.Velocities.Average()).
		Msgf("loaded the CSV file %s", cliValues.Filename())

	return nil
}

func init() {
	l = logger.New(logger.DevelopmentEnvironment(), logger.LogLevelDebug())
	fs := afero.NewOsFs()
	afs = &afero.Afero{Fs: fs}
}

func readCsv(filename string, afs *afero.Afero) ([]string, error) {
	f, err := afs.Open(filename)
	if err != nil {
		return make([]string, 0), errors.Wrap(err, "could not open the CSV file")
	}
	defer func(f afero.File) {
		_ = f.Close()
	}(f)

	scanner := bufio.NewScanner(f)
	var lines []string
	for scanner.Scan() {
		line := scanner.Text()
		lines = append(lines, line)
	}

	return lines, nil
}

func withVelocities(file labradar.CsvFileContents) series.MutatorFunction {
	return func(s *series.Series) {
		for i := labradar.LineNumberVelocityStart; i < len(file.Lines); i++ {
			v := file.GetIntValue(i)
			s.AppendVelocity(i-labradar.LineNumberVelocityStart, v)
		}
	}
}

func withSeriesNumber(file labradar.CsvFileContents) series.MutatorFunction {
	return func(s *series.Series) {
		s.Number = file.GetIntValue(labradar.LineNumberSeries)
	}
}

//func AddMuzzleVelocitiesFromCsv(file domain.LabradarCsvFile) MutatorFunction {
//	return func(s *Series) {
//		for i := domain.LineNumberVelocityStart; i < len(file.Lines); i++ {
//			velocity := file.GetIntValue(i)
//			s.Velocities.Values = append(s.Velocities.Values, velocity)
//		}
//	}
//}
//
//// WithSeriesNumberFromCsv will extract the device ID from the CSV file.
//func WithSeriesNumberFromCsv(f domain.LabradarCsvFile) MutatorFunction {
//	return func(s *Series) {
//		s.Number = f.GetIntValue(domain.LineNumberSeries)
//	}
//}
