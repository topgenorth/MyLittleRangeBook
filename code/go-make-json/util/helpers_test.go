package util

import (
	"opgenorth.net/labradar/fs"
	"testing"
)

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
		{"Stripped the last NUL (unicode)", args{"Remove the unicode NUL" + fs.UnicodeNUL}, "Remove the unicode NUL"},
		{"Stripped the last NUL (hex)", args{"Remove the hex NUL" + fs.HexNUL}, "Remove the hex NUL"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := TrimLastChar(tt.args.s); got != tt.want {
				t.Errorf("TrimLastChar() = %v, want %v", got, tt.want)
			}
		})
	}
}

