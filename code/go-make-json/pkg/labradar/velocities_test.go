package labradar

import (
	"opgenorth.net/labradar/pkg/util"
	"testing"
)

func Test_average_of_array(t *testing.T) {
	tests := []struct {
		name string
		args []int
		want float64
	}{
		// TODO: Add test cases.
		{"Should calculate CalculateAverage for velocities", []int{1200, 1247, 1190, 1222}, 1214.8},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := util.CalculateAverage(tt.args); got != tt.want {
				t.Errorf("CalculateAverage() = %v, want %v", got, tt.want)
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
			min, max := util.GetMaxAndMin(tt.args)
			if min != tt.wantMin {
				t.Errorf("GetMaxAndMin() min = %v, want %v", min, tt.wantMin)
			}
			if max != tt.wantMax {
				t.Errorf("GetMaxAndMin() max = %v, want %v", max, tt.wantMax)
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
			if got := util.CalculateStandardDeviation(tt.args); got != tt.want {
				t.Errorf("CalculateStandardDeviation() = %v, want %v", got, tt.want)
			}
		})
	}
}