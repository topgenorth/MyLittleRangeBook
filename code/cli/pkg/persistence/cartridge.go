package persistence

import (
	"fmt"
	"github.com/google/uuid"
	"gorm.io/gorm"
)

type Cartridge struct {
	gorm.Model
	BoreDiameter float64   `gorm:"index:idx_cartridges__borediameter,not null"`
	Name         string    `gorm:"index:idx_cartridges__name,unique,not null"`
	Size         string    `gorm:"index:idx_cartridges__size,not null"`
	UUID         uuid.UUID `gorm:"index:idx_cartridges__uuid,unique,not null"`
}

func (c Cartridge) String() string {
	return fmt.Sprintf("%s (%s)", c.Name, c.Size)
}

type CartridgesGORM struct {
	db        *gorm.DB
	RecentErr error
}

func Cartridges() *CartridgesGORM {
	gormDb, err := getDb()

	if err != nil {
		return &CartridgesGORM{
			db:        nil,
			RecentErr: err,
		}
	}
	return &CartridgesGORM{
		db:        gormDb,
		RecentErr: nil,
	}
}

func (c CartridgesGORM) NewCartridge(name string, size string, bore float64) Cartridge {
	return Cartridge{
		Name:         name,
		Size:         size,
		BoreDiameter: bore,
		UUID:         uuid.New(),
	}
}
func (c CartridgesGORM) SaveCartridge(cartridge Cartridge) {

	var tx *gorm.DB
	if cartridge.ID < 1 {
		tx = c.db.Create(&cartridge)
	} else {
		tx = c.db.Save(&cartridge)
	}

	if tx.Error != nil {
		c.RecentErr = handleSqlite3Error(c.RecentErr)
	} else {
		c.RecentErr = nil
	}

}

// GetAll will return an array of persistence.Cartridge that is sorted by the bore diameter.
func (c CartridgesGORM) GetAll() []Cartridge {

	var cartridges []Cartridge
	if result := c.db.Order("bore_diameter asc").Find(&cartridges); result.Error != nil {
		c.RecentErr = result.Error
		return nil
	}

	return cartridges
}

func (c CartridgesGORM) DeleteById(id uint) {
	tx := c.db.Delete(&Cartridge{}, id)
	c.RecentErr = handleSqlite3Error(tx.Error)
}
func (c CartridgesGORM) Delete(cartridge *Cartridge) {
	if result := c.db.Delete(cartridge); result.Error != nil {
		c.RecentErr = result.Error
	}
}
