package labradar

import (
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/test"
	"reflect"
	"testing"
)

func TestDirectory_SeriesNumbers(t *testing.T) {
	const testFsDir = Directory(LBRDirectory)
	type args struct {
		afs afero.Fs
	}

	testFs := test.InitLabradarFilesystemForTest()
	emptyList := make([]SeriesNumber, 0)
	testFsArgs := args{afs: testFs}

	tests := []struct {
		name string
		d    Directory
		args args
		want []SeriesNumber
	}{
		{
			name: "Should return an empty list of SeriesNumbers for empty Directory.",
			d:    EmptyLabradarDirectory,
			args: testFsArgs,
			want: emptyList,
		},
		{
			name: "Should get list of SeriesNumbers.",
			d:    testFsDir,
			args: testFsArgs,
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
