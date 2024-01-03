package labradar

import (
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/test"
	"path/filepath"
	"reflect"
	"testing"
)

func TestDirectory_SeriesNumbers(t *testing.T) {
	type args struct {
		afs afero.Fs
	}

	testFs := test.InitLabradarFilesystemForTest()
	emptyList := make([]SeriesNumber, 0)

	tests := []struct {
		name string
		d    Directory
		args args
		want []SeriesNumber
	}{
		{
			name: "Should return an empty list of SeriesNumbers for empty Directory.",
			d:    EmptyLabradarDirectory,
			args: args{testFs},
			want: emptyList,
		},
		{
			name: "Should get list of SeriesNumbers.",
			d:    Directory(test.LBRDirectory),
			args: args{testFs},
			want: []SeriesNumber{SeriesNumber(1), SeriesNumber(2), SeriesNumber(3), SeriesNumber(4), SeriesNumber(5), SeriesNumber(6)},
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := tt.d.SeriesNumbers(tt.args.afs); !reflect.DeepEqual(got, tt.want) {
				t.Errorf("SeriesNumbers() = %v, want %v", got, tt.want)
			}
		})
	}
}

func Test_tryParseDirectoryPath(t *testing.T) {
	testFs := test.InitLabradarFilesystemForTest()
	type args struct {
		dir string
		fs  afero.Fs
	}

	tests := []struct {
		name    string
		args    args
		want    Directory
		wantErr bool
	}{
		{name: "Should successfully parse the directory path.",
			args:    args{test.LBRDirectory, testFs},
			want:    Directory(test.LBRDirectory),
			wantErr: false},
		{name: "Should return empty Directory for an empty path.",
			args:    args{"", testFs},
			want:    Directory(""),
			wantErr: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got, err := tryParseDirectoryPath(tt.args.dir, tt.args.fs)
			if (err != nil) != tt.wantErr {
				t.Errorf("tryParseDirectoryPath() error = %v, wantErr %v", err, tt.wantErr)
				return
			}
			if got != tt.want {
				t.Errorf("tryParseDirectoryPath() got = %v, want %v", got, tt.want)
			}
		})
	}
}

func Test_getLabradarMarkerFile(t *testing.T) {
	testFs := test.InitLabradarFilesystemForTest()
	type args struct {
		path string
		af   afero.Fs
	}
	tests := []struct {
		name    string
		args    args
		want    string
		wantErr bool
	}{
		{
			name:    "Should find LID file.",
			args:    args{test.LBRDirectory, testFs},
			want:    filepath.Join(test.LBRDirectory, test.LIDFile),
			wantErr: false,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got, err := getLabradarMarkerFile(tt.args.path, tt.args.af)
			if (err != nil) != tt.wantErr {
				t.Errorf("getLabradarMarkerFile() error = %v, wantErr %v", err, tt.wantErr)
				return
			}
			if got != tt.want {
				t.Errorf("getLabradarMarkerFile() got = %v, want %v", got, tt.want)
			}
		})
	}
}
