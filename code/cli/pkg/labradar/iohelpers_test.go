package labradar

import (
	"opgenorth.net/mylittlerangebook/pkg"
	"testing"
)

func Test_isLabradarSeriesDir(t *testing.T) {
	tests := []struct {
		name  string
		value string
		want  bool
	}{
		{
			name:  "Valid Labradar OldSeries name",
			value: "SR0001",
			want:  true,
		},
		{
			name:  "Labradar OldSeries doesn't start with SR",
			value: "TS0001",
			want:  false,
		},
		{
			name:  "Labradar OldSeries starts with sr",
			value: "sr0001",
			want:  false,
		},
		{
			name:  "Labradar OldSeries name too short",
			value: "SR001",
			want:  false,
		},
		{
			name:  "Labradar OldSeries name too long",
			value: "SR00001",
			want:  false,
		},
		{
			name:  "Labradar OldSeries doesn't end with numbers",
			value: "SR00o1",
			want:  false,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := isSeriesDirectory(tt.value); got != tt.want {
				t.Errorf("isSeriesDirectory() = %v, want %v", got, tt.want)
			}
		})
	}
}
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
			if got := SanitizeLine(tt.args.line); got != tt.want {
				t.Errorf("sanitizeLine() = %v, want %v", got, tt.want)
			}
		})
	}
}
