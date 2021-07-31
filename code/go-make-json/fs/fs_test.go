package fs

import "testing"

func TestGetPathToLabradarSeries(t *testing.T) {
	type args struct {
		seriesNumber int
	}
	tests := []struct {
		name string
		args args
		want string
	}{
		// TODO: Add test cases.
		{"Generate path for series 42", args{seriesNumber: 42}, "/Users/tom/work/labradar/LBR/SR0042/SR0042 Report.csv"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := GetPathToLabradarSeries(tt.args.seriesNumber); got != tt.want {
				t.Errorf("GetPathToLabradarSeries() = %v, want %v", got, tt.want)
			}
		})
	}
}
