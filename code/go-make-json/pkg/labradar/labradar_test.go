package labradar

import (
	"opgenorth.net/labradar/pkg"
	"testing"
)

func Test_fixupLineFromLabradarFile(t *testing.T) {
	type args struct {
		line string
	}
	tests := []struct {
		name string
		args args
		want string
	}{
		// TODO: Add test cases.
		{"Fix up a line that terminates with a NUL and whitespace", args{"Device ID;LBR-0013797;;" + pkg.UnicodeNUL + "                                     "}, "Device ID;LBR-0013797;;"},
		{"Fix up a line that begins with NUL and whitespace", args{pkg.UnicodeNUL + "Device ID;LBR-0013797;;"}, "Device ID;LBR-0013797;;"},
		{"Fix up a line that terminates with a NUL, ends with a NUL, and whitespace", args{pkg.UnicodeNUL + "Device ID;LBR-0013797;;" + pkg.UnicodeNUL + "                                     "}, "Device ID;LBR-0013797;;"},
		{"Fix up a line that doesn't need it", args{"Device ID;LBR-0013797;;"}, "Device ID;LBR-0013797;;"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := fixupLabradarLine(tt.args.line); got != tt.want {
				t.Errorf("fixupLabradarLine() = %v, want %v", got, tt.want)
			}
		})
	}
}
