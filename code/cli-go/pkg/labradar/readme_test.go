package labradar

import (
	"reflect"
	"testing"
)

func Test_getReadmeLine(t *testing.T) {
	type args struct {
		text string
	}
	tests := []struct {
		name string
		args args
		want *ReadmeLine
	}{
		// TODO: Add test cases.
		{"Should create ReadMe line with no errors",
			args{"| 001 | 8mm Mauser; 180gr Barnes TSX; 45.0 gr IMR 4895 | Isreali 98K | 2020-07-25 |\n"},
			&ReadmeLine{
				LineNumber:   0,
				SeriesNumber: 1,
				Firearm:      &Firearm{"Isreali 98K", "8mm Mauser"},
				Load: &LoadData{
					Cartridge: "8mm Mauser",
					Projectile: &Projectile{
						Name:   "180gr Barnes TSX",
						Weight: 0,
						BC:     nil,
					},
					Powder: &PowderCharge{
						Name:   "45.0 gr IMR 4895",
						Amount: 0,
					},
				},
				Text: "| 001 | 8mm Mauser; 180gr Barnes TSX; 45.0 gr IMR 4895 | Isreali 98K | 2020-07-25 |",
				Err:  nil,
			},
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := getReadmeLine(tt.args.text); !reflect.DeepEqual(got, tt.want) {
				t.Errorf("getReadmeLine() = %v, want %v", got, tt.want)
			}
		})
	}
}
