package mylittlerangebook

import (
	"fmt"
	log "github.com/sirupsen/logrus"
	"opgenorth.net/labradar/pkg/config"
	"opgenorth.net/labradar/pkg/labradar"
	"opgenorth.net/labradar/pkg/model/cartridge"
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

func (a *MyLittleRangeBook) ListCartridges() {

	cartridges, err := cartridge.FetchAll()
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

func (a *MyLittleRangeBook) GetLabradarSeries(cfg *labradar.ReadCsvConfig) (*labradar.Series, error) {
	cfg.Config = a.Config
	r := labradar.NewCsvConversion(cfg)

	if r.Error != nil {
		return nil, r.Error
	}

	return r.LabradarSeries, nil
}
