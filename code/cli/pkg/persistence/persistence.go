package persistence

import (
	"fmt"
	"gorm.io/driver/sqlite"
	"gorm.io/gorm"
)

type Cartridge struct {
	gorm.Model
	BoreDiameter float64 `gorm:"index:idx_cartridges__borediameter,not null"`
	Name         string  `gorm:"index:idx_cartridges__name,unique,not null"`
	Size         string  `gorm:"index:idx_cartridges__size"`
}

func (t Cartridge) String() string {
	return fmt.Sprintf("%s (%s)", t.Name, t.Size)
}

func NewCartridge(name string, size string, bore float64) Cartridge {
	return Cartridge{
		Name:         name,
		Size:         size,
		BoreDiameter: bore,
	}
}

func GetCartridges() ([]Cartridge, error) {
	var c []Cartridge
	db, err := gorm.Open(sqlite.Open("mlrb.db3"), &gorm.Config{})
	if err != nil {
		return nil, err
	}

	if result := db.Find(c); result.Error != nil {
		return nil, result.Error
	}

	return c, nil
}

func UpsertCartridge(c Cartridge) error {
	err := Bootstrap()
	if err != nil {
		return err
	}

	db, err := gorm.Open(sqlite.Open("mlrb.db3"), &gorm.Config{})
	if err != nil {
		return err
	}
	db.Create(&c)

	return nil

}
func Bootstrap() error {
	db, err := gorm.Open(sqlite.Open("mlrb.db3"), &gorm.Config{})
	if err != nil {
		return err
	}

	// Migrate the schema
	err = db.AutoMigrate(&Cartridge{})
	if err != nil {
		return err
	}

	return nil
}
