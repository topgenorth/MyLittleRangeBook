package mlrb

import (
	"fmt"
	log "github.com/sirupsen/logrus"
	"io"
	"opgenorth.net/mylittlerangebook/pkg/cloud"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"sort"
	"strings"
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
	log.SetFormatter(&log.TextFormatter{})
	if a.Config.Debug {
		log.Infoln("Debugging: true")
		log.SetLevel(log.TraceLevel)
	} else {
		log.Infoln("Debugging: false")
		log.SetLevel(log.InfoLevel)
	}
}

// ListCartridges will do a simple dump of the cartridges to STDOUT.
func (a *MyLittleRangeBook) ListCartridges() {

	cartridges, err := cloud.FetchAllCartridges()
	if err != nil {
		log.Error("Problem retrieving a list of cartridges. ", err)
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

// ReadLabradarCsv will take a Labradar CSV file, and display relevant details to STDOUT.
func (a *MyLittleRangeBook) ReadLabradarCsv(inputDir string, seriesNumber int) (*labradar.Series, error) {
	r := labradar.LoadCsv(a.Config, inputDir, seriesNumber)

	if r.Error != nil {
		return nil, fmt.Errorf("could not read the Labradar file %s, %v", labradar.FilenameForSeries(inputDir, seriesNumber), r.Error)
	}

	return r.Series, nil
}

func (a *MyLittleRangeBook) SubmitLabradarCsv(filename string) error {
	err := cloud.SubmitLabradarCsvFile(filename)
	if err != nil {
		return fmt.Errorf("error submitting the Labradar file %s, %v", filename, err)
	}
	return nil
}

func (a *MyLittleRangeBook) ListLabradarCsvFiles(inputDir string) ([]labradar.CsvFile, error) {
	files := labradar.LoadDataFiles(a.Config, inputDir)

	fmt.Printf("Labradar files in %s:\n", inputDir)
	for _, f := range files.Files {
		fmt.Println(strings.ReplaceAll(f.String(), inputDir, " * "))
	}
	fmt.Printf("Done.\n")
	return nil, nil
}

func (a *MyLittleRangeBook) SubmitCartridge(name string, size string) (*cloud.Cartridge, error) {
	c, err := cloud.AddCartridge(name, size)
	if err != nil {
		return nil, err
	}
	return c, nil
}
