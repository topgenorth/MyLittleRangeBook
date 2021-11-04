package labradar
//
//import (
//	"opgenorth.net/labradar/pkg/model/cartridge"
//	"testing"
//)
//
//func Test_isLabradarCsvFile(t *testing.T) {
//	type lineArgs struct {
//		path string
//	}
//	tests := []struct {
//		name string
//		lineArgs lineArgs
//		want bool
//	}{
//		// TODO: Add test cases.
//		{"Is a Labradar CSV file",
//			lineArgs {"/Users/tom/work/labradar/LBR/SR0001/SR0001 Report.csv"},
//			true,
//		},
//		{"Is not a Labradar CSV file - invalid extension",
//			lineArgs {"/Users/tom/work/labradar/LBR/SR0001/SR0001 Report.CSX"},
//			false,
//		},
//		{"Is not a Labradar CSV file - doesn't start with SR",
//			lineArgs {"/Users/tom/work/labradar/LBR/SR0001/S0001 Report.CSV"},
//			false,
//		},
//	}
//	for _, tt := range tests {
//		t.Run(tt.name, func(t *testing.T) {
//			if got := cartridge.isLabradarCsvFile(tt.lineArgs.path); got != tt.want {
//				t.Errorf("isLabradarCsvFile() = %v, want %v", got, tt.want)
//			}
//		})
//	}
//}
