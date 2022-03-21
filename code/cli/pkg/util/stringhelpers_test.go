package util

import (
	"opgenorth.net/mylittlerangebook/pkg"
	"reflect"
	"testing"
	"time"
)

func Test_TrimLastChar(t *testing.T) {
	type args struct {
		s string
	}
	tests := []struct {
		name string
		args args
		want string
	}{
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

func Test_ToTime(t *testing.T) {
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
			if got := ToTime(tt.args.d, tt.args.t); !reflect.DeepEqual(got, tt.want) {
				t.Errorf("ToTime() = %v, want %v", got, tt.want)
			}
		})
	}
}

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
		{
			name:      "numeric with space",
			str:       "01234 56789",
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

func TestPadLeft(t *testing.T) {
	type args struct {
		v      int64
		length int
	}
	tests := []struct {
		name string
		args args
		want string
	}{
		{
			name: "Should return a string of 4 characters: 1",
			args: args{1, 4},
			want: "0001",
		},
		{
			name: "Should return a string of 4 characters: 9999",
			args: args{9999, 4},
			want: "9999",
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := PadLeft(tt.args.v, tt.args.length); got != tt.want {
				t.Errorf("PadLeft() = %v, want %v", got, tt.want)
			}
		})
	}
}
