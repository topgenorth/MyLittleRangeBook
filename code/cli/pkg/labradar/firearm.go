package labradar

import "fmt"

type Firearm struct {
	Name      string `json:"name"`
	Cartridge string `json:"cartridge"`
}

func (t Firearm) String() string {
	return fmt.Sprintf("%s (%s)", t.Name, t.Cartridge)
}
