package labradar

type Projectile struct {
	Name   string                `json:"name"`
	Weight int                   `json:"weight"`
	BC     *BallisticCoefficient `json:"bc"`
}
