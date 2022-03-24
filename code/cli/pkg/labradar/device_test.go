package labradar

import (
	"github.com/sirupsen/logrus"
	"testing"
)

func Test_getDeviceId(t *testing.T) {
	type args struct {
		filename string
	}
	tests := []struct {
		name string
		args args
		want DeviceId
	}{
		{
			name: "Parse valid filename.",
			args: args{
				filename: "LBR0013797201909141617.LID",
			},
			want: DeviceId("LBR-0013797"),
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			t.Fail()

			//if got := getDeviceId(tt.args.filename); got != tt.want {
			//	t.Errorf("getDeviceId() = %v, want %v", got, tt.want)
			//}
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

func Test_UpdateDeviceForSeries(t *testing.T) {

	logrus.Warnln("This test is intentionally disabled.")
	t.Fail()

	//d := &Device{
	//	DeviceId:  "LBR-1234567",
	//	TimeZone:  pkg.DefaultTimeZone,
	//	Directory: "",
	//	af:        aferox.NewAferox(pwd, afero.NewMemMapFs()),
	//}

	//type args struct {
	//	device *Device
	//}
	//tests := []struct {
	//	name string
	//	args args
	//	want series.SeriesMutatorFn
	//}{
	//
	//
	//}
	//for _, tt := range tests {
	//	t.Run(tt.name, func(t *testing.T) {
	//		if got := UpdateDeviceForSeries(tt.args.device); !reflect.DeepEqual(got, tt.want) {
	//			t.Errorf("UpdateDeviceForSeries() = %v, want %v", got, tt.want)
	//		}
	//	})
	//}
}
