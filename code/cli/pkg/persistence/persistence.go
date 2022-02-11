package persistence

import (
	"gorm.io/driver/sqlite"
	"gorm.io/gorm"
)

type Cartridge struct {
	gorm.Model
	Name      string
	Cartridge string
	Size      string
}

func NewCartridge(n string, c string, s string) Cartridge {
	return Cartridge{
		Name:      n,
		Cartridge: c,
		Size:      s,
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
