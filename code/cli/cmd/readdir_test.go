package cmd

import "testing"

func Test_getSeriesNumberFrom(t *testing.T) {
	type args struct {
		path string
	}
	tests := []struct {
		name string
		args args
		want int
	}{
		// TODO: Add test cases.
		{
			"Should return 42 for the series number",
			args{"/Users/tom/work/labradar/LBR/SR0042/SR0042 Report.CSV"},
			42,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := getSeriesNumberFrom(tt.args.path); got != tt.want {
				t.Errorf("getSeriesNumberFrom() = %v, want %v", got, tt.want)
			}
		})
	}
}
