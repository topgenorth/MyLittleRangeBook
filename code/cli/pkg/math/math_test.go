package math

import (
	"testing"
)

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

func TestGetMaxAndMinForInts(t *testing.T) {
	tests := []struct {
		name    string
		args    []int
		wantMin int
		wantMax int
	}{
		{"Should get max and min from array", []int{1200, 1247, 1190, 1222}, 1190, 1247},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			min, max := GetMinAndMaxForInts(tt.args)
			if min != tt.wantMin {
				t.Errorf("GetMinAndMaxForInts() min = %v, want %v", min, tt.wantMin)
			}
			if max != tt.wantMax {
				t.Errorf("GetMinAndMaxForInts() max = %v, want %v", max, tt.wantMax)
			}
		})
	}
}

type intTestingStruct struct {
	name         string
	valuesToTest []int
	want         float64
}

func TestCalculateAverageOfInts(t *testing.T) {
	tests := []intTestingStruct{
		{"Should calculate CalculateAverageOfInts for velocities", []int{1200, 1247, 1190, 1222}, 1214.8},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := CalculateAverageOfInts(tt.valuesToTest); got != tt.want {
				t.Errorf("CalculateAverageOfInts() = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestCalculateStdDevOfInts(t *testing.T) {
	tests := []intTestingStruct{
		{"Test 1", []int{1200, 1247, 1190, 1222}, 21.9},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := CalculateStdDevOfInts(tt.valuesToTest); got != tt.want {
				t.Errorf("CalculateStdDevOfInts() = %v, want %v", got, tt.want)
			}
		})
	}
}
