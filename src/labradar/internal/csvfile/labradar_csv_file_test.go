package csvfile

import (
	"github.com/spf13/afero"
	constants "opgenorth.net/tomutil"
	"reflect"
	"testing"
)

//sr0011_reportcsv is a file that was copied from the device.  It is a byte of strings, and had some garbage/junk it in
// like Unicode NULL characters.

func Test_loadCsv(t *testing.T) {
	afs := &afero.Afero{Fs: afero.NewMemMapFs()}
	_ = afs.WriteFile("./testdata/series1.csv", []byte("test file"), 0755)

	type args struct {
		path string
		afs  *afero.Afero
	}

	tests := []struct {
		name      string
		args      args
		want      []string
		wantError bool
	}{
		{
			name: "Should open and read from file.",
			args: args{
				path: "./testdata/series1.csv",
				afs:  afs,
			},
			want:      []string{"test file"},
			wantError: false,
		},
		{
			name: "File does not exist should should have an error.",
			args: args{
				path: "./testdata/no-such-file.csv",
				afs:  afs,
			},
			want:      nil,
			wantError: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			csv := LoadCsv(tt.args.path, tt.args.afs)

			if !reflect.DeepEqual(csv.Lines, tt.want) {
				t.Errorf("did not read the contents of the file.")
			}

			if tt.wantError && csv.Err == nil {
				t.Errorf("Received an error where none was expected.")
			} else if !tt.wantError && csv.Err != nil {
				t.Error("Did not receive an error where one was expected.")
			}

		})
	}
}

func Test_sanitizeLineFromLabradarCSV(t *testing.T) {
	type args struct {
		line string
	}
	tests := []struct {
		name string
		args args
		want string
	}{
		{"Fix up a line that terminates with a NUL and whitespace",
			args{"OldDevice ID;LBR-0013797;;" + constants.UnicodeNUL + "                                     "},
			"OldDevice ID;LBR-0013797;;",
		},
		{"Fix up a line that begins with NUL and whitespace",
			args{constants.UnicodeNUL + "OldDevice ID;LBR-0013797;;"},
			"OldDevice ID;LBR-0013797;;",
		},
		{"Fix up a line that terminates with a NUL, ends with a NUL, and whitespace",
			args{constants.UnicodeNUL + "OldDevice ID;LBR-0013797;;" + constants.UnicodeNUL + "                                     "},
			"OldDevice ID;LBR-0013797;;",
		},
		{"Fix up a line that doesn't need it",
			args{"OldDevice ID;LBR-0013797;;"},
			"OldDevice ID;LBR-0013797;;",
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := sanitizeLineFromLabradarCSV(tt.args.line); got != tt.want {
				t.Errorf("sanitizeLine() = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestLabradarCsvFile_GetIntValue(t *testing.T) {
	type fields struct {
		inputFilename string
		Err           error
		Lines         []string
	}
	type args struct {
		lineNumber int
	}
	tests := []struct {
		name   string
		fields fields
		args   args
		want   int
	}{
		// TODO: Add test cases.
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			csv := &CsvFileContents{
				inputFilename: tt.fields.inputFilename,
				Err:           tt.fields.Err,
				Lines:         tt.fields.Lines,
			}
			if got := csv.GetIntValue(tt.args.lineNumber); got != tt.want {
				t.Errorf("GetIntValue() = %v, want %v", got, tt.want)
			}
		})
	}
}
