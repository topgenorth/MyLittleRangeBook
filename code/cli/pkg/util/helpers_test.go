package util

import (
	"opgenorth.net/mylittlerangebook/pkg"
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
		{"Stripped the last NUL (unicode)", args{"Remove the unicode NUL" + pkg.UnicodeNUL}, "Remove the unicode NUL"},
		{"Stripped the last NUL (hex)", args{"Remove the hex NUL" + pkg.HexNUL}, "Remove the hex NUL"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := TrimLastChar(tt.args.s); got != tt.want {
				t.Errorf("TrimLastChar() = %v, want %v", got, tt.want)
			}
		})
	}
}

//func Test_IsNumericOnly(t *testing.T) {
//	cases := []struct {
//		name      string
//		str       string
//		isNumeric bool
//	}{
//		{
//			name:      "numeric string",
//			str:       "0123456789",
//			isNumeric: true,
//		},
//		{
//			name:      "not numeric string",
//			str:       "#0123456789",
//			isNumeric: false,
//		},
//	}
//
//	for _, c := range cases {
//		t.Run(c.name, func(t *testing.T) {
//			assert.Equal(t, c.isNumeric, IsNumericOnly(c.str))
//		})
//	}
//}

func TestIsNumericOnly(t *testing.T) {
	tests := []struct {
		name      string
		str       string
		isNumeric bool
	}{
		{
			name:      "numeric string",
			str:       "0123456789",
			isNumeric: true,
		},
		{
			name:      "not numeric string",
			str:       "#0123456789",
			isNumeric: false,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := IsNumericOnly(tt.str); got != tt.isNumeric {
				t.Errorf("IsNumericOnly() = %v, want %v", got, tt.isNumeric)
			}
		})
	}
}
