package series

import "testing"

func TestNumber_String(t *testing.T) {
	tests := []struct {
		name string
		t    Number
		want string
	}{
		{"Should return formatted name.", 55, "SR0055"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := tt.t.String(); got != tt.want {
				t.Errorf("String() = %v, want %v", got, tt.want)
			}
		})
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := tt.t.SeriesName(); got != tt.want {
				t.Errorf("SeriesName() = %v, want %v", got, tt.want)
			}
		})
	}
}
