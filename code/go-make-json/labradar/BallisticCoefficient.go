package labradar

type BallisticCoefficient struct {
	DragModel string  `json:"dragModel"`
	Value     float32 `json:"value"`
}
