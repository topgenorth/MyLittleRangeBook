package persistence

import (
	"github.com/mattn/go-sqlite3"
	"github.com/sirupsen/logrus"
	"gorm.io/driver/sqlite"
	"gorm.io/gorm"
)

const (
	DatabaseName = "mlrb.db3"
)

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
		logrus.WithError(sqliteError).Debug("Unexpected error.")
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
