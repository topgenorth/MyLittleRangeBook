package labradar

import (
	"opgenorth.net/mylittlerangebook/pkg"
	"testing"
)

func Test_sanitizeLine(t *testing.T) {
	type args struct {
		line string
	}
	tests := []struct {
		name string
		args args
		want string
	}{
		{"Fix up a line that terminates with a NUL and whitespace",
			args{"OldDevice ID;LBR-0013797;;" + pkg.UnicodeNUL + "                                     "},
			"OldDevice ID;LBR-0013797;;",
		},
		{"Fix up a line that begins with NUL and whitespace",
			args{pkg.UnicodeNUL + "OldDevice ID;LBR-0013797;;"},
			"OldDevice ID;LBR-0013797;;",
		},
		{"Fix up a line that terminates with a NUL, ends with a NUL, and whitespace",
			args{pkg.UnicodeNUL + "OldDevice ID;LBR-0013797;;" + pkg.UnicodeNUL + "                                     "},
			"OldDevice ID;LBR-0013797;;",
		},
		{"Fix up a line that doesn't need it",
			args{"OldDevice ID;LBR-0013797;;"},
			"OldDevice ID;LBR-0013797;;",
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := sanitizeLineFromLabradarCSV(tt.args.line); got != tt.want {
				t.Errorf("sanitizeLine() = %v, want %v", got, tt.want)
			}
		})
	}
}
