package labradar

import (
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/test"
	"reflect"
	"testing"
)

func TestDirectory_SeriesNumbers(t *testing.T) {

	testFs := test.InitLabradarFilesystemForTest()

	emptyList := make([]SeriesNumber, 0)

	type args struct {
		afs afero.Fs
	}
	tests := []struct {
		name string
		d    Directory
		args args
		want []SeriesNumber
	}{
		{
			name: "Should return an empty list of SeriesNumbers for empty Directory.",
			d:    EmptyLabradarDirectory,
			args: args{afs: testFs},
			want: emptyList,
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
