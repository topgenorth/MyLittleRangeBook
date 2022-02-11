package mlrb

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"io"
	"log"
	"opgenorth.net/mylittlerangebook/pkg/cloud"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar/device"
	"opgenorth.net/mylittlerangebook/pkg/labradar/fs"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series/jsonwriter"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series/summarywriter"
	"opgenorth.net/mylittlerangebook/pkg/persistence"
	"sort"
)

type MyLittleRangeBook struct {
	*config.Config
}

// New will return a pointer to a new mlrb.MyLittleRangeBook structure.
func New(cfg *config.Config) *MyLittleRangeBook {
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

// Device will return a new device.Device struct using the provided LBR directory.
func (a *MyLittleRangeBook) Device(lbrDir string) (*device.Device, error) {
	d, err := device.New(lbrDir, a.Filesystem, a.Timezone)
	return d, err
}

// ReadLabradarSeries will take a Labradar CSV file, and display relevant details to STDOUT.
func (a *MyLittleRangeBook) ReadLabradarSeries(lbrDirectory string, seriesNumber int) (*series.LabradarSeries, error) {

	n := series.Number(seriesNumber)
	d, err := a.Device(lbrDirectory)
	if err != nil {
		return nil, err
	}

	b, err := d.HasSeries(n)
	if err != nil {
		return nil, err
	}
	if !b {
		return nil, series.NewNotFoundError(lbrDirectory, seriesNumber)
	}

	s, err := d.LoadSeries(n)
	if err != nil {
		return nil, err
	}

	return s, nil
}

func (a *MyLittleRangeBook) DisplaySeries(series series.LabradarSeries, verbose bool) error {

	var w summarywriter.SummaryWriter
	if verbose {
		w = summarywriter.New(a.Out, summarywriter.DescriptivePlainText)
	} else {
		w = summarywriter.New(a.Out, summarywriter.SimplePlainText)
	}

	if err := w.Write(series); err != nil {
		return err
	}

	return nil
}

// SaveLabradarSeriesToJson will write the series.LabradarSeries to a JSON file in the specified directory.
func (a *MyLittleRangeBook) SaveLabradarSeriesToJson(dir string, series *series.LabradarSeries) error {

	filename := jsonwriter.DefaultJsonFileProvider(dir, series.Number)

	exists, err := a.Filesystem.Exists(filename())
	if err != nil {
		return err
	}
	if exists {
		if err = a.Filesystem.Remove(filename()); err != nil {
			return err
		}
		logrus.Debugf("Deleting the file `%s`.", filename())
	}

	w := jsonwriter.New(a.Filesystem, func() string { return filename() })
	if err := w.Write(*series); err != nil {
		return err
	}

	return nil
}

// SubmitLabradarCsv file will upload the CSV file to cloud storage.
func (a *MyLittleRangeBook) SubmitLabradarCsv(filename string) error {
	err := cloud.SubmitLabradarCsvFile(filename)
	if err != nil {
		return fmt.Errorf("error submitting the Labradar file %s: %w", filename, err)
	}
	return nil
}

// GetListOfLabradarFiles will display all the CSV files in the LBR directory.
func (a *MyLittleRangeBook) GetListOfLabradarFiles(lbrDirectory string) ([]string, error) {
	files := fs.ListLabradarSeriesReportFiles(lbrDirectory, a.Filesystem)
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

func (a *MyLittleRangeBook) SaveCartridge(n string, c string, s string) error {
	cartridge := persistence.NewCartridge(n, c, s)
	err := persistence.UpsertCartridge(cartridge)
	if err != nil {
		return err
	}
	return nil
}

func configureLogging(a *MyLittleRangeBook) {
	if a.Config.Debug {
		logrus.SetReportCaller(true)
		logrus.SetLevel(logrus.TraceLevel)
	} else {
		logrus.SetLevel(logrus.WarnLevel)
	}

	logrus.SetFormatter(&logrus.TextFormatter{})
	logrus.Tracef("Debugging: %t", a.Debug)
}
