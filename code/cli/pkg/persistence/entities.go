package persistence

import (
	"fmt"
	"gorm.io/gorm"
)

type Cartridge struct {
	gorm.Model
	BoreDiameter float64 `gorm:"index:idx_cartridges__borediameter,not null"`
	Name         string  `gorm:"index:idx_cartridges__name,unique,not null"`
	Size         string  `gorm:"index:idx_cartridges__size,not null"`
}

func (t Cartridge) String() string {
	return fmt.Sprintf("%s (%s)", t.Name, t.Size)
}
