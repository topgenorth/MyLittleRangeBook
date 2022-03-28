package describeseries

import (
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"reflect"
	"testing"
)

func Test_parse_string_makes_a_projectile(t *testing.T) {
	type args struct {
		projectile string
	}
	tests := []struct {
		name string
		args args
		want *labradar.Projectile
	}{
		{
			name: "format 1",
			args: args{projectile: "123gr Hornady ELD Match"},
			want: &labradar.Projectile{Name: "Hornady ELD Match", Weight: 123, BC: nil},
		},
		{
			name: "format 2",
			args: args{projectile: "85 grain Speer SP"},
			want: &labradar.Projectile{Name: "Speer SP", Weight: 85, BC: nil},
		},
		{
			name: "format 3",
			args: args{projectile: "120 gr Hornady SST"},
			want: &labradar.Projectile{Name: "Hornady SST", Weight: 120, BC: nil},
		},
		{
			name: "format 4",
			args: args{projectile: "120 Hornady SST"},
			want: &labradar.Projectile{Name: "Hornady SST", Weight: 120, BC: nil},
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := parseProjectileString(tt.args.projectile); !reflect.DeepEqual(got, tt.want) {
				t.Errorf("parseProjectileString() = %v, want %v", got, tt.want)
			}
		})
	}
}

func Test_parseWeightFromProjectileString(t *testing.T) {
	tests := []struct {
		name   string
		str    string
		weight int
	}{
		{
			name:   "Simple integer",
			str:    "120",
			weight: 120,
		},
		{
			name:   "Simple integer with starting space",
			str:    " 120",
			weight: 120,
		},
		{
			name:   "Simple integer with trailing space",
			str:    "120 ",
			weight: 120,
		},
		{
			name:   "Simple integer with unit of measure 1",
			str:    "120gr",
			weight: 120,
		},
		{
			name:   "Simple integer with unit of measure 2",
			str:    "120 gr",
			weight: 120,
		},
		{
			name:   "Simple decimal with unit of measure 3",
			str:    "120.0 gr",
			weight: 120,
		},
		{
			name:   "Simple decimal with unit of measure 4",
			str:    "120.0 gr.",
			weight: 120,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := parseWeightFromProjectileString(tt.str); got != tt.weight {
				t.Errorf("parseWeightFromProjectileString() = %v, want %v", got, tt.weight)
			}
		})
	}
}

func Test_parseNameOfProjectileFromString(t *testing.T) {
	tests := []struct {
		name string
		str  string
		want string
	}{
		{
			name: "Just the name, no weight",
			str:  "Hornady ELD Match",
			want: "Hornady ELD Match",
		},
		{
			name: "with grain at the start",
			str:  " grain Speer SP",
			want: "Speer SP",
		},
		{
			name: "with gr at the start",
			str:  " gr Hornady SST",
			want: "Hornady SST",
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := parseNameOfProjectileFromString(tt.str); got != tt.want {
				t.Errorf("parseNameOfProjectileFromString() = %v, want %v", got, tt.want)
			}
		})
	}
}
