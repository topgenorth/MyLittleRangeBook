package labradar

type LoadData struct {
	Cartridge  string        `json:"cartridge"`
	Projectile *Projectile   `json:"projectile"`
	Powder     *PowderCharge `json:"powder"`
}
