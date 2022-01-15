package fs

import (
	. "github.com/onsi/ginkgo"
)
import . "github.com/onsi/gomega"
import "testing"

var _ = Describe("isLabradarCsvFile", func() {

	Describe("Check if Labradar CSV ", func() {

		Context("Has correct prefix and extension", func() {
			It("should pass.", func() {
				Expect(isLabradarCsvFile("SR0001 Report.csv")).To(BeTrue())
			})
		})
		Context("Has invalid extension", func() {
			It("should not pass", func() {
				Expect(isLabradarCsvFile("/tmp/data/LBR/SR0001/SR0001 Report.txt")).To(BeFalse())
			})
		})
		Context("Does not start with SR", func() {
			It("should not pass", func() {
				Expect(isLabradarCsvFile("/tmp/data/LBR/SR0001/0001 Report.csv")).To(BeFalse())
			})
		})
	})
})

func Test_isLabradarCsvFile(t *testing.T) {
	tests := []struct {
		name string
		path string
		want bool
	}{
		{
			name: "Valid Labradar Series file (filename only)",
			path: "SR0001 Report.csv",
			want: true,
		},
		{
			name: "Does not end with csv - Invalid",
			path: "SR0001 Report.txt",
			want: false,
		},
		{
			name: "Ends with CSV - Invalid",
			path: "SR0001 Report.CSV",
			want: false,
		},
		{
			name: "Filename does not end with ` Report` - Invalid",
			path: "SR0001 Report - original shooting order.CSV",
			want: false,
		},
		{
			name: "Filename does not start with SR - Invalid",
			path: "TS0001 Report.csv",
			want: false,
		},
		{
			name: "Filename starts with sr - Invalid",
			path: "sr0001 Report.csv",
			want: false,
		},
		{
			name: "Filename series is not numeric - Invalid",
			path: "SR00o1 Report.csv",
			want: false,
		},
		{
			name: "Filename series is not numeric - Invalid",
			path: "/tmp/data/LBR/SR0001/SR0001 Report.csv",
			want: false,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := isLabradarCsvFile(tt.path); got != tt.want {
				t.Errorf("isLabradarCsvFile() = %v, want %v", got, tt.want)
			}
		})
	}
}

func Test_isLabradarCsvFile_using_BDD(t *testing.T) {
	RegisterFailHandler(Fail)
	RunSpecs(t, "isLabradarCsvFile")
}

func TestFilenameForSeries2(t *testing.T) {
	type args struct {
		labradarDirectory string
		seriesNumber      int
	}
	tests := []struct {
		name string
		args args
		want string
	}{
		{"Should format the name correctly",
			args{"/tmp/LBR", 1},
			"/tmp/LBR/SR0001/SR0001 Report.csv",
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := FilenameForSeries(tt.args.labradarDirectory, tt.args.seriesNumber); got != tt.want {
				t.Errorf("FilenameForSeries() = %v, want %v", got, tt.want)
			}
		})
	}
}
