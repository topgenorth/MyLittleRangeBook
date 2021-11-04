package labradar

import (
	"reflect"
	"testing"
)

func TestNewLineOfData(t *testing.T) {
	type lineArgs struct {
		linenumber int
		raw        string
	}

	tests := []struct {
		name string
		args lineArgs
		want string
	}{
		{
			name: "Get the device ID from line 1.",
			args: lineArgs{linenumber: 1, raw: "Device ID;LBR-0013797;;\u0000                                     \n"},
			want: "Device ID;LBR-0013797;;",
		},
		{
			name: "Get the series number from line number 3.",
			args: lineArgs{linenumber: 3, raw: "\u0000Series No;0001;;\u0000                                            \n"},
			want: "Series No;0001;;",
		},
		{
			name: "Get the number of shots number from line number 4.",
			args: lineArgs{linenumber: 4, raw: "\u0000Total number of shots;0009;;\u0000                                \n"},
			want: "Total number of shots;0009;;",
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := NewLineOfData(tt.args.linenumber, tt.args.raw); !reflect.DeepEqual(got, tt.want) {
				t.Errorf("NewLineOfData() = `%v`, want `%v`", got, tt.want)
			}
		})
	}
}
