package labradar

import (
	"opgenorth.net/labradar/util"
	"testing"
)

func TestFormatLabradarSeriesNumber(t *testing.T) {
	type args struct {
		seriesNumber int
	}
	tests := []struct {
		name string
		args args
		want string
	}{
		// TODO: Add test cases.
		{"Should should format series 42.", args{seriesNumber: 42}, "SR0042"},
		{"Should should format series 141.", args{seriesNumber: 141}, "SR0141"},
		{"Should should format series 13s41.", args{seriesNumber: 1341}, "SR1341"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := util.FormatLabradarSeriesNumber(tt.args.seriesNumber); got != tt.want {
				t.Errorf("FormatLabradarSeriesNumber() = %v, want %v", got, tt.want)
			}
		})
	}
}

func Test_initLabradarStruct(t *testing.T) {
	ls := initLabradarStruct(42)

	if ls.SeriesName != "SR0042" {
		t.Errorf("ls.SeriesName = %s; wanted SR0042.", ls.SeriesName)
	}

}
