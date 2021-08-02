package util

import "testing"

func Test_trimLastChar(t *testing.T) {
	type args struct {
		s string
	}
	tests := []struct {
		name string
		args args
		want string
	}{
		// TODO: Add test cases.
		{"Stripped the last character", args{"Remove the Plus+"}, "Remove the Plus"},
		{"Stripped the last NUL (unicode)", args{"Remove the unicode NUL" + UnicodeNUL}, "Remove the unicode NUL"},
		{"Stripped the last NUL (hex)", args{"Remove the hex NUL" + HexNUL}, "Remove the hex NUL"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := TrimLastChar(tt.args.s); got != tt.want {
				t.Errorf("TrimLastChar() = %v, want %v", got, tt.want)
			}
		})
	}
}

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
		{"Fix up a line that terminates with a NUL and whitespace", args{"Device ID;LBR-0013797;;" + UnicodeNUL + "                                     "}, "Device ID;LBR-0013797;;"},
		{"Fix up a line that begins with NUL and whitespace", args{UnicodeNUL + "Device ID;LBR-0013797;;"}, "Device ID;LBR-0013797;;"},
		{"Fix up a line that terminates with a NUL, ends with a NUL, and whitespace", args{UnicodeNUL + "Device ID;LBR-0013797;;" + UnicodeNUL + "                                     "}, "Device ID;LBR-0013797;;"},
		{"Fix up a line that doesn't need it", args{"Device ID;LBR-0013797;;"}, "Device ID;LBR-0013797;;"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := FixupLabradarLine(tt.args.line); got != tt.want {
				t.Errorf("FixupLabradarLine() = %v, want %v", got, tt.want)
			}
		})
	}
}
