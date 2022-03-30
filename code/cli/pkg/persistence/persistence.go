package persistence

//
//import (
//	"github.com/mattn/go-sqlite3"
//	"github.com/sirupsen/logrus"
//	"gorm.io/driver/sqlite"
//	"gorm.io/gorm"
//)
//
//const (
//	DatabaseName = "mlrb.db3"
//)
//
//var gormConfig *gorm.Config
//
//func init() {
//
//	gormConfig = &gorm.Config{}
//	err := Bootstrap()
//	if err != nil {
//		logrus.WithError(err).Panicf("Could not bootstrap the database %s.", DatabaseName)
//		return
//	}
//
//}
//
//func handleSqlite3Error(err error) error {
//	if err == nil {
//		return nil
//	}
//
//	sqliteError, ok := err.(sqlite3.Error)
//	if !ok {
//		return err
//	}
//	switch sqliteError.Code {
//	case sqlite3.ErrConstraint:
//		if sqliteError.ExtendedCode == 2067 {
//			logrus.WithError(sqliteError).Debug("Cartridge exists.")
//			return nil
//		}
//	default:
//		logrus.WithError(sqliteError).Debug("Unexpected error.")
//		return sqliteError
//	}
//
//	return err
//}
//
//func getDb() (*gorm.DB, error) {
//	db, err := gorm.Open(sqlite.Open(DatabaseName), gormConfig)
//	if err != nil {
//		return nil, err
//	}
//	return db, nil
//}
//
//func Bootstrap() error {
//	db, err := getDb()
//	if err != nil {
//		return err
//	}
//
//	// Migrate the schema
//	// TODO [TO20220222] Make an array of entities.
//	err = db.AutoMigrate(&Cartridge{}, &Series{})
//	if err != nil {
//		return err
//	}
//
//	return nil
//}
