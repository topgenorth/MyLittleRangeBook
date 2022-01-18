package cmd

import "opgenorth.net/mylittlerangebook/pkg/labradar/series"

// describeParameters holds the values that are necessary to describe a give Labradar series.
type describeParameters struct {
	seriesNumber int
	notes        string
	inputDir     string
	firearm      string
	cartridge    string
	bullet       string
	powder       string
	cbto         float32
}

func (p *describeParameters) updateSeries(s series.LabradarSeries) {

	// TODO [TO20220118] Setup the mutators and apply them.

	//s.Notes = p.notes
	////s.Firearm.Name = p.firearm
	////s.LoadData.Cartridge = p.cartridge
	////s.LoadData.CBTO = p.cbto
	//
	//s.SetProjectile(p.bullet)
	//s.SetPowder(p.powder)

}
