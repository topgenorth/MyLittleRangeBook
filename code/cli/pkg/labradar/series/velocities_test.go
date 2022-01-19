package series

import (
	"opgenorth.net/mylittlerangebook/pkg/math"
	"testing"
)

func Test_average_of_array(t *testing.T) {
	tests := []struct {
		name string
		args []int
		want float64
	}{
		// TODO: Add test cases.
		{"Should calculate CalculateAverageOfInts for velocities", []int{1200, 1247, 1190, 1222}, 1214.8},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := math.CalculateAverageOfInts(tt.args); got != tt.want {
				t.Errorf("CalculateAverageOfInts() = %v, want %v", got, tt.want)
			}
		})
	}
}

func Test_getMaxAndMin_of_array(t *testing.T) {
	tests := []struct {
		name    string
		args    []int
		wantMin int
		wantMax int
	}{
		// TODO: Add test cases.
		{"Should get max and min from array", []int{1200, 1247, 1190, 1222}, 1190, 1247},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			min, max := math.GetMinAndMaxForInts(tt.args)
			if min != tt.wantMin {
				t.Errorf("GetMinAndMaxForInts() min = %v, want %v", min, tt.wantMin)
			}
			if max != tt.wantMax {
				t.Errorf("GetMinAndMaxForInts() max = %v, want %v", max, tt.wantMax)
			}
		})
	}
}

func Test_standardDeviation_for_array(t *testing.T) {
	tests := []struct {
		name string
		args []int
		want float64
	}{
		// TODO: Add test cases.
		{"Test 1", []int{1200, 1247, 1190, 1222}, 21.9},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := math.CalculateStdDevOfInts(tt.args); got != tt.want {
				t.Errorf("CalculateStdDevOfInts() = %v, want %v", got, tt.want)
			}
		})
	}
}
