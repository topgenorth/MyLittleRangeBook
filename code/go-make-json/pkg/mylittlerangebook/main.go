package mylittlerangebook

import (
	"fmt"
	"github.com/spf13/viper"
	"opgenorth.net/labradar/pkg/model/cartridge"
	"sort"
)

type App struct {
	viper *viper.Viper
}

func New() (*App, error) {
	v := viper.New()
	v.AddConfigPath(".")

	err := v.ReadInConfig()
	if err != nil {
		return nil, err
	}

	return &App{viper: v}, nil
}

func (a *App) ListCartridges() {

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
