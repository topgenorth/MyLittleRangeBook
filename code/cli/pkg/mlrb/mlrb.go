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

func (a *MyLittleRangeBook) ShowConfig() {
	log.Info("Show Config")
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
func (a *MyLittleRangeBook) ReadLabradarCsv(cfg *labradar.ReadCsvConfig) (*labradar.Series, error) {
	cfg.Config = a.Config
	r := labradar.ReadFile(cfg)

	if r.Error != nil {
		return nil, fmt.Errorf("Could not read the Labradar file %s %v", cfg.GetInputFilename(), r.Error)
	}

	return r.LabradarSeries, nil
}

func (a *MyLittleRangeBook) SubmitLabradarCsv(cfg *labradar.ReadCsvConfig) error {
	err := cloud.SubmitLabradarCsvFile(cfg.GetInputFilename())
	if err != nil {
		return fmt.Errorf("Error submitting the Labradar file #%s, %v", cfg.GetInputFilename(), err)
	}
	return nil
}
