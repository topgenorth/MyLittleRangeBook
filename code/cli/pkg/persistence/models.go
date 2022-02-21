package persistence

type Cartridges struct {
}

func (c Cartridges) New(name string, size string, bore float64) Cartridge {
	return Cartridge{
		Name:         name,
		Size:         size,
		BoreDiameter: bore,
	}
}
func (c Cartridges) Save(cartridge Cartridge) error {
	db, err := getDb()
	if err != nil {
		return err
	}

	dbc := db.Create(&cartridge)
	return handleSqlite3Error(dbc.Error)
}
func (c Cartridges) GetAll() ([]Cartridge, error) {
	db, err := getDb()

	if err != nil {
		return nil, err
	}

	var cartridges []Cartridge
	if result := db.Order("bore_diameter asc").Find(&cartridges); result.Error != nil {
		return nil, result.Error
	}

	return cartridges, nil
}
