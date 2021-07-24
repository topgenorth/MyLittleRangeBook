package domain

type LabradarSeries struct {
	Labradar Labradar
	Firearm  Firearm
	LoadData LoadData
	Notes    string
	Tags     []string
}
