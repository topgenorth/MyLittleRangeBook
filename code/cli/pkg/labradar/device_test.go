package labradar

import "testing"

func Test_getDeviceId(t *testing.T) {
	type args struct {
		filename string
	}
	tests := []struct {
		name string
		args args
		want string
	}{
		{
			name: "Parse valid filename.",
			args: args{
				filename: "LBR0013797201909141617.LID",
			},
			want: "LBR-0013797",
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := getDeviceId(tt.args.filename); got != tt.want {
				t.Errorf("getDeviceId() = %v, want %v", got, tt.want)
			}
		})
	}
}

func Test_looksLikeTheLabradarMarkerFile(t *testing.T) {
	type args struct {
		filename string
	}
	tests := []struct {
		name string
		args args
		want bool
	}{
		{
			name: "Is a valid filename so passes.",
			args: args{
				filename: "LBR0013797201909141617.LID",
			},
			want: true,
		},
		{
			name: "Does not have correct extension so fails.",
			args: args{
				filename: "LBR0013797201909141617.TXT",
			},
			want: false,
		},
		{
			name: "Does not have start with LBR so fails.",
			args: args{
				filename: "xxx0013797201909141617.LID",
			},
			want: false,
		},
		{
			name: "Starts with lbr so fails",
			args: args{
				filename: "lbr0013797201909141617.LID",
			},
			want: false,
		},
		{
			name: "Filename is too short so fails.",
			args: args{
				filename: "LBR001379720190914161.LID",
			},
			want: false,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := looksLikeTheLabradarMarkerFile(tt.args.filename); got != tt.want {
				t.Errorf("looksLikeTheLabradarMarkerFile() = %v, want %v", got, tt.want)
			}
		})
	}
}
