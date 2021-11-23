package mlrb

import (
	"fmt"
	log "github.com/sirupsen/logrus"
	"opgenorth.net/mylittlerangebook/pkg/cloud"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
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
		fmt.Println(c.String())
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

	for _, f := range files.Files {
		fmt.Println(f)
	}

	//files, err := os.ReadDir(inputDir)
	//if err != nil {
	//	log.Fatal(err)
	//}
	//
	//for _, file := range files {
	//	if file.IsDir() && strings.HasPrefix(file.Name(), "SR") {
	//		fmt.Println(file.Name())
	//	}
	//}

	return nil, nil
}
