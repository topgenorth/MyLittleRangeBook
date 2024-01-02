package catalog

import (
	"labreader/pkg/timeprovider"
	"reflect"
	"testing"
	"time"
)

func Test_timestampDestinationFile(t *testing.T) {
	dateForTest := time.Date(2023, time.December, 12, 10, 04, 0, 0, time.UTC)
	timeProviderForTest := timeprovider.New(
		timeprovider.WithDate(dateForTest),
		timeprovider.WithDateLayout(dateLayoutForFilePrefix))

	type args struct {
		filePath string
	}
	tests := []struct {
		name string
		args args
		want string
	}{
		{
			name: "Has a valid date so don't prefix the filename",
			args: args{filePath: `C:\Users\tom.opgenorth\Dropbox\Firearms\MyLogs\20181029-M305-DataBook.pdf`},
			want: `20181029-M305-DataBook.pdf`,
		},
		{
			name: "Has a valid future date so prefix the filename",
			args: args{filePath: `C:\Users\tom.opgenorth\Dropbox\Firearms\MyLogs\30001029-M305-DataBook.pdf`},
			want: timeProviderForTest.String() + `-30001029-M305-DataBook.pdf`,
		},
		{
			name: "Has a invalid date so prefix the filename",
			args: args{filePath: `C:\Users\tom.opgenorth\Dropbox\Firearms\MyLogs\30001345-M305-DataBook.pdf`},
			want: timeProviderForTest.String() + `-30001345-M305-DataBook.pdf`,
		},
		{
			name: "Doesn't start with what appears to be a date so prefix the filename",
			args: args{filePath: `C:\Users\tom.opgenorth\Dropbox\Firearms\MyLogs\M305-DataBook.pdf`},
			want: timeProviderForTest.String() + `-M305-DataBook.pdf`,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := timestampDestinationFile(tt.args.filePath, timeProviderForTest); got != tt.want {
				t.Errorf("timestampDestinationFile() = %v, want %v", got, tt.want)
			}
		})
	}
}

func Test_parseDateyyyyMMdd(t *testing.T) {
	type args struct {
		dateString string
	}
	tests := []struct {
		name    string
		args    args
		want    time.Time
		wantErr bool
	}{
		{
			"Valid date layout so should return a date",
			args{"20181029"},
			time.Date(2018, time.October, 29, 0, 0, 0, 0, time.UTC),
			false,
		},
		{
			"Invalid date should return an error",
			args{"20181asdf"},
			time.Date(1, 1, 1, 0, 0, 0, 0, time.UTC),
			true,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got, err := parseDateyyyyMMdd(tt.args.dateString)
			if (err != nil) != tt.wantErr {
				t.Errorf("parseDateyyyyMMdd() error = %v, wantErr %v", err, tt.wantErr)
				return
			}
			if !reflect.DeepEqual(got, tt.want) {
				t.Errorf("parseDateyyyyMMdd() got = %v, want %v", got, tt.want)
			}
		})
	}
}
