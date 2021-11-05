package mlrb

import (
	"fmt"
	log "github.com/sirupsen/logrus"
	"opgenorth.net/mylittlerangebook/pkg/aws"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"os"
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

	cartridges, err := aws.FetchAllCartridges(*a.Config)
	if err != nil {
		log.Error("Problem retrieving a list of cartridges. ", err)
		return
	}

	sort.Slice(cartridges[:], func(i, j int) bool {
		return cartridges[i].Name < cartridges[j].Name
	})

	for _, c := range cartridges {
		fmt.Println(c.ToString())
	}
}

func (a *MyLittleRangeBook) ReadLabradarCsv(cfg *labradar.ReadCsvConfig) (*labradar.Series, error) {
	cfg.Config = a.Config
	r := labradar.ReadFile(cfg)

	if r.Error != nil {
		return nil, r.Error
	}

	return r.LabradarSeries, nil
}

// Init will create the necessary filesystem structure.
func (a *MyLittleRangeBook) Init(dir string) {

	if _, err := os.Stat(dir); err == nil {
		// path/to/whatever exists// path/to/whatever exists
		log.Debugf("The directory %s exists.", dir)
	} else if os.IsNotExist(err) {
		log.Warnf("Have to create the directory %s.", dir)
	} else {
		log.Fatal(err)
		// Schrodinger: file may or may not exist. See err for details.

		// Therefore, do *NOT* use !os.IsNotExist(err) to test for file existence
	}

	fmt.Println("Initializing directories....")
}
