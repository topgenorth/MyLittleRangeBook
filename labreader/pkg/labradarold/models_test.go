package labradarold

import "testing"

func TestPowderCharge_String(t *testing.T) {
	tests := []struct {
		name string
		pc   PowderCharge
		want string
	}{
		{
			name: "Should format correctly",
			pc: PowderCharge{
				Name:   "H-4895",
				Amount: 45.5,
			},
			want: "45.5gr H-4895",
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			pc := PowderCharge{
				Name:   tt.pc.Name,
				Amount: tt.pc.Amount,
			}
			if got := pc.String(); got != tt.want {
				t.Errorf("String() = %v, want %v", got, tt.want)
			}
		})
	}
}
