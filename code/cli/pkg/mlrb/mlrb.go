package mlrb

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"io"
	"log"
	"opgenorth.net/mylittlerangebook/pkg/cloud"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"opgenorth.net/mylittlerangebook/pkg/labradar/fs"
	"sort"
)

type MyLittleRangeBook struct {
	*config.Config
}

func New() *MyLittleRangeBook {
	cfg := config.New()
	return NewWithConfig(cfg)
}

func NewWithConfig(cfg *config.Config) *MyLittleRangeBook {
	return &MyLittleRangeBook{
		cfg,
	}
}

func (a *MyLittleRangeBook) ConfigLogging() {
	logrus.SetFormatter(&logrus.TextFormatter{})
	if a.Config.Debug {
		logrus.Infoln("Debugging: true")
		logrus.SetLevel(logrus.TraceLevel)
	} else {
		logrus.Infoln("Debugging: false")
		logrus.SetLevel(logrus.InfoLevel)
	}
}

// ListCartridges will do a simple dump of the cartridges on record to STDOUT.
func (a *MyLittleRangeBook) ListCartridges() {

	cartridges, err := cloud.FetchAllCartridges()
	if err != nil {
		logrus.Error("Problem retrieving a list of cartridges. ", err)
	}
	sort.Slice(cartridges[:], func(i, j int) bool {
		return cartridges[i].Name < cartridges[j].Name
	})

	for _, c := range cartridges {
		_, err := io.WriteString(a.Config.Out, c.String())
		if err != nil {
			log.Fatal(err)
		}
	}
}

// LoadLabradarCsv will take a Labradar CSV file, and display relevant details to STDOUT.
func (a *MyLittleRangeBook) LoadLabradarCsv(inputDir string, seriesNumber int) (*labradar.Series, error) {

	//filename := fs.File
	//s, err := labradar.LoadSeries(, a.Config.FileSystem)
	//r := labradar.LoadCsv(a.Config, inputDir, seriesNumber)
	//
	//if err != nil {
	//	return nil, fmt.Errorf("could not read the Labradar file %s, %w: ", lbrio.FilenameForSeries(inputDir, seriesNumber), r.Error)
	//}
	//
	//return r.Series, nil

	filename := fs.FilenameForSeries(inputDir, seriesNumber)
	s, err := labradar.LoadSeries(filename, a.Config.FileSystem)
	if err != nil {
		return nil, fmt.Errorf("could not retrieve series %d in %s: w", seriesNumber, inputDir, err)
	}

	return s, nil
}

// SubmitLabradarCsv file will upload the CSV file to cloud storage.
func (a *MyLittleRangeBook) SubmitLabradarCsv(filename string) error {
	err := cloud.SubmitLabradarCsvFile(filename)
	if err != nil {
		return fmt.Errorf("error submitting the Labradar file %s: %w", filename, err)
	}
	return nil
}

// GetListOfLabradarFiles will display all the CSV files in the Labradar directory.
func (a *MyLittleRangeBook) GetListOfLabradarFiles() ([]string, error) {

	return nil, nil
}

// SubmitCartridge will add a new cartridge to the cartridges on record.
func (a *MyLittleRangeBook) SubmitCartridge(name string, size string) (*cloud.Cartridge, error) {
	c, err := cloud.AddCartridge(name, size)
	if err != nil {
		return nil, err
	}
	return c, nil
}
