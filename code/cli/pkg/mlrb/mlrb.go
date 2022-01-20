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
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"sort"
)

type MyLittleRangeBook struct {
	*config.Config
}

// New will return a pointer to a new mlrb.MyLittleRangeBook structure.
func New() *MyLittleRangeBook {
	cfg := config.New()
	app := &MyLittleRangeBook{
		cfg,
	}
	configureLogging(app)
	return app
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

// LoadSeriesFromLabradar will take a Labradar CSV file, and display relevant details to STDOUT.
func (a *MyLittleRangeBook) LoadSeriesFromLabradar(inputDir string, seriesNumber int) (*series.LabradarSeries, error) {

	device, err := labradar.NewDevice(inputDir, a.AppContext)
	if err != nil {
		return nil, fmt.Errorf("could not retrieve series %d from '%s': %w", seriesNumber, inputDir, err)
	}

	s, err := device.LoadSeries(seriesNumber)
	if err != nil {
		return nil, fmt.Errorf("could not retrieve series %d in %s: %w", seriesNumber, inputDir, err)
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
func (a *MyLittleRangeBook) GetListOfLabradarFiles(inputDir string) ([]string, error) {
	files := fs.ListLabradarFiles(inputDir, a.Filesystem)
	return files, nil
}

// SubmitCartridge will add a new cartridge to the cartridges on record.
func (a *MyLittleRangeBook) SubmitCartridge(name string, size string) (*cloud.Cartridge, error) {
	c, err := cloud.AddCartridge(name, size)
	if err != nil {
		return nil, err
	}
	return c, nil
}

func configureLogging(a *MyLittleRangeBook) {
	logrus.SetFormatter(&logrus.TextFormatter{})
	if a.Config.Debug {
		logrus.Infoln("Debugging: true")
		logrus.SetLevel(logrus.TraceLevel)
	} else {
		logrus.SetLevel(logrus.InfoLevel)
	}
}
