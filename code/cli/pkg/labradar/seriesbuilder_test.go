package labradar

import "testing"

func TestSeriesBuilder_ParseLine_01(t *testing.T) {

	b := NewSeriesBuilder()

	l := NewLineOfData(1, "OldDevice ID;LBR-0013797;;\u0000                                     \n")

	b.ParseLine(l)
}
