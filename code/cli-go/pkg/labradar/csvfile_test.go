package labradar

import (
	"reflect"
	"testing"
)

func TestReadFile(t *testing.T) {
	type args struct {
		c *ReadCsvConfig
	}
	tests := []struct {
		name string
		args args
		want *CsvFile
	}{
		{
			"SHould provide filename",
			args{c: nil},
			&CsvFile{
				InputFile:      "/Users/tom/work/mlrb/LBR/SR0001/SR0001 Report.csv",
				LabradarSeries: nil,
				Error:          nil,
			},
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := ReadFile(tt.args.c); !reflect.DeepEqual(got, tt.want) {
				t.Errorf("ReadFile() = %v, want %v", got, tt.want)
			}
		})
	}
}
