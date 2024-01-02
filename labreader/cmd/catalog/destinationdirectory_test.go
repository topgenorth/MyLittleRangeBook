package catalog

import (
	constants "labreader/internal"
	"testing"
)

func Test_destinationDirectory(t *testing.T) {
	type args struct {
		vals     commandLineValues
		filename string
	}

	rifleCartridge := args{
		filename: "",
		vals: commandLineValues{
			Rifle:        "700SPS",
			Cartridge:    ".223",
			Powder:       "",
			PowderCharge: 0,
			Bullet:       "",
			BulletWeight: 0,
			COAL:         0,
			CBTO:         0,
			Rename:       false,
			Dryrun:       false,
		},
	}

	rifleCartridgePowder := args{
		filename: "",
		vals: commandLineValues{
			Rifle:        "700SPS",
			Cartridge:    ".223",
			Powder:       "BL-C(2)",
			PowderCharge: 24.5,
			Bullet:       "",
			BulletWeight: 0,
			COAL:         0,
			CBTO:         0,
			Rename:       false,
			Dryrun:       false,
		},
	}

	rifleCartridgeBullet := args{
		filename: "",
		vals: commandLineValues{
			Rifle:        "700SPS",
			Cartridge:    ".223",
			Powder:       constants.UnknownStr,
			PowderCharge: 0,
			Bullet:       "Campro",
			BulletWeight: 55,
			COAL:         0,
			CBTO:         0,
			Rename:       false,
			Dryrun:       false,
		},
	}

	rifleCartridgeBulletPowder := args{
		filename: "",
		vals: commandLineValues{
			Rifle:        "700SPS",
			Cartridge:    ".223",
			Powder:       "BL-C(2)",
			PowderCharge: 24.5,
			Bullet:       "Campro",
			BulletWeight: 55,
			COAL:         0,
			CBTO:         0,
			Rename:       false,
			Dryrun:       false,
		},
	}

	tests := []struct {
		name string
		args args
		want string
	}{
		{name: "Correct directory for rifle and cartridge", args: rifleCartridge, want: "C:\\Users\\tom.opgenorth\\Dropbox\\Firearms\\MyLogs\\700SPS\\223"},
		{name: "Correct directory for rifle, cartridge, and powder", args: rifleCartridgePowder, want: `C:\Users\tom.opgenorth\Dropbox\Firearms\MyLogs\700SPS\223\BL-C(2)-24_5`},
		{name: "Correct directory for rifle, cartridge, and bullet", args: rifleCartridgeBullet, want: `C:\Users\tom.opgenorth\Dropbox\Firearms\MyLogs\700SPS\223\Campro-55`},
		{name: "Correct directory for rifle, cartridge, powder, and bullet", args: rifleCartridgeBulletPowder, want: `C:\Users\tom.opgenorth\Dropbox\Firearms\MyLogs\700SPS\223\Campro-55\BL-C(2)-24_5`},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := destinationDirectory(tt.args.vals); got != tt.want {
				t.Errorf("destinationDirectory() = %v, want %v", got, tt.want)
			}
		})
	}
}
