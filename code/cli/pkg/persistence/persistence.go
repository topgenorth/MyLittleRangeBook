package persistence

import (
	"gorm.io/driver/sqlite"
	"gorm.io/gorm"
)

type Cartridge struct {
	gorm.Model
	Name string `gorm:"index:idx_cartridges__name,unique,not null"`
	Size string `gorm:"index:idx_cartridges__size"`
}

func NewCartridge(name string, size string) Cartridge {
	return Cartridge{
		Name: name,
		Size: size,
	}
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
