package fs

import "testing"

func Test_isLabradarSeriesDir(t *testing.T) {
	tests := []struct {
		name  string
		value string
		want  bool
	}{
		{
			name:  "Valid Labradar OldSeries name",
			value: "SR0001",
			want:  true,
		},
		{
			name:  "Labradar OldSeries doesn't start with SR",
			value: "TS0001",
			want:  false,
		},
		{
			name:  "Labradar OldSeries starts with sr",
			value: "sr0001",
			want:  false,
		},
		{
			name:  "Labradar OldSeries name too short",
			value: "SR001",
			want:  false,
		},
		{
			name:  "Labradar OldSeries name too long",
			value: "SR00001",
			want:  false,
		},
		{
			name:  "Labradar OldSeries doesn't end with numbers",
			value: "SR00o1",
			want:  false,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := isLabradarSeriesName(tt.value); got != tt.want {
				t.Errorf("isLabradarSeriesName() = %v, want %v", got, tt.want)
			}
		})
	}
}
