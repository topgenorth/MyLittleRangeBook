package fs

import (
	"opgenorth.net/mylittlerangebook/pkg"
	"reflect"
	"testing"
	"time"
)

func Test_toTime(t *testing.T) {
	type args struct {
		d string
		t string
	}
	tests := []struct {
		name string
		args args
		want time.Time
	}{
		{"Should handle an afternoon hour in the 24 hour clock.",
			args{"07-30-2020", "19:05:02"},
			time.Date(2020, 7, 30, 19, 05, 02, 0, time.UTC),
		},
		{"Should handle an morning hour in the 24 hour clock.",
			args{"07-30-2020", "11:05:02"},
			time.Date(2020, 7, 30, 11, 05, 02, 0, time.UTC),
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := toTime(tt.args.d, tt.args.t); !reflect.DeepEqual(got, tt.want) {
				t.Errorf("toTime() = %v, want %v", got, tt.want)
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
			if got := sanitizeLine(tt.args.line); got != tt.want {
				t.Errorf("sanitizeLine() = %v, want %v", got, tt.want)
			}
		})
	}
}
