package persistence

import (
	"fmt"
	"github.com/mattn/go-sqlite3"
	"github.com/sirupsen/logrus"
	"gorm.io/driver/sqlite"
	"gorm.io/gorm"
)

const (
	DatabaseName = "mlrb.db3"
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

func NewCartridge(name string, size string, bore float64) Cartridge {
	return Cartridge{
		Name:         name,
		Size:         size,
		BoreDiameter: bore,
	}
}

func GetCartridges() ([]Cartridge, error) {
	var c []Cartridge
	db, err := getDb()

	if err != nil {
		return nil, err
	}

	if result := db.Find(c); result.Error != nil {
		return nil, result.Error
	}

	return c, nil
}

func UpsertCartridge(c Cartridge) error {
	db, err := getDb()
	if err != nil {
		return err
	}

	dbc := db.Create(&c)
	return handleSqlite3Error(dbc.Error)
}

func handleSqlite3Error(err error) error {
	if err == nil {
		return nil
	}

	sqliteError, ok := err.(sqlite3.Error)
	if !ok {
		return err
	}
	switch sqliteError.Code {
	case sqlite3.ErrConstraint:
		if sqliteError.ExtendedCode == 2067 {
			logrus.WithError(sqliteError).Debug("Cartridge exists.")
			return nil
		}
	default:
		return sqliteError
	}

	return err
}

func getDb() (*gorm.DB, error) {
	err := Bootstrap()
	if err != nil {
		return nil, err
	}

	db, err := gorm.Open(sqlite.Open(DatabaseName), &gorm.Config{})
	if err != nil {
		return nil, err
	}
	return db, nil
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
