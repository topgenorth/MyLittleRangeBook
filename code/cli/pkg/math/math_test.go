package math

import "testing"

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
