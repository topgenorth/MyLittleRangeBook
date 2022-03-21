package labradar

import "testing"

func TestSeriesNumber_String(t *testing.T) {
	tests := []struct {
		name string
		t    SeriesNumber
		want string
	}{
		{"Should return formatted series name for 9999", SeriesNumber(9999), "SR9999"},
		{"Should return formatted series name for 0", SeriesNumber(0), "SR0000"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := tt.t.String(); got != tt.want {
				t.Errorf("String() = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestSeriesNumber_ReportCsv(t *testing.T) {
	tests := []struct {
		name string
		t    SeriesNumber
		want string
	}{
		{"Should return Report CSV name.", SeriesNumber(9999), "SR9999 Report.csv"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := tt.t.ReportCsv(); got != tt.want {
				t.Errorf("ReportCsv() = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestSeriesNumber_LbrName(t *testing.T) {
	tests := []struct {
		name string
		t    SeriesNumber
		want string
	}{
		{"Should generate .lbr filename.", SeriesNumber(9999), "SR9999.lbr"},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := tt.t.LbrName(); got != tt.want {
				t.Errorf("LbrName() = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestTryParseSeriesNumber(t *testing.T) {

	const emptySN = SeriesNumber(0)
	type args struct {
		sr string
	}
	tests := []struct {
		name  string
		args  args
		want  SeriesNumber
		want1 bool
	}{
		{"Should parse SerialNumber", args{"SR0001"}, SeriesNumber(1), true},
		{"Should not parse all strings", args{"XXXXXX"}, emptySN, false},
		{"Should not parse a string longer than 6", args{"SR00001"}, emptySN, false},
		{"Should not parse a string less than 6", args{"SR001"}, emptySN, false},
		{"Should not parse a string that does not start with SR", args{"AB0000"}, emptySN, false},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got, got1 := TryParseSeriesNumber(tt.args.sr)
			if got != tt.want {
				t.Errorf("TryParseSeriesNumber() got = %v, want %v", got, tt.want)
			}
			if got1 != tt.want1 {
				t.Errorf("TryParseSeriesNumber() got1 = %v, want %v", got1, tt.want1)
			}
		})
	}
}
