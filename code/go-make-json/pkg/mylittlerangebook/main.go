package mylittlerangebook

import (
	"fmt"
	"opgenorth.net/labradar/pkg/config"
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

func (a *MyLittleRangeBook) ListCartridges() {

	cartridges, err := cartridge.FetchAll()
	if err != nil {
		fmt.Println("Problem retrieving a list of cartridges. ", err)
		return
	}

	sort.Slice(cartridges[:], func(i, j int) bool {
		return cartridges[i].Name < cartridges[j].Name
	})

	for _, c := range cartridges {
		fmt.Println(c.ToString())
	}
}

func (a *MyLittleRangeBook) ConvertLabradarCsvToJson(inputFile string) (string, error) {
	fmt.Println(inputFile)
	return "", nil
}
