package series

import "fmt"

// BallisticCoefficient captures the ballistics data about a specific projectile.
type BallisticCoefficient struct {
	DragModel string  `json:"dragModel"`
	Value     float32 `json:"value"`
}

func (t BallisticCoefficient) String() string {
	return fmt.Sprintf("%s 0.3%f", t.DragModel, t.Value)
}

type Projectile struct {
	Name   string                `json:"name"`
	Weight int                   `json:"weight"`
	BC     *BallisticCoefficient `json:"bc"`
}

func (t Projectile) String() string {
	return fmt.Sprintf("%dgr %s", t.Weight, t.Name)
}
