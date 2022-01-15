package labradar

import (
	. "github.com/onsi/ginkgo"
	"opgenorth.net/mylittlerangebook/pkg/labradar/fs"
)
import . "github.com/onsi/gomega"
import "testing"

var _ = Describe("isLabradarCsvFile", func() {

	Describe("Check if Labradar CSV ", func() {

		Context("Has correct prefix and extension", func() {
			It("should pass.", func() {
				Expect(isLabradarCsvFile("/tmp/data/LBR/SR0001/SR0001 Report.csv")).To(BeTrue())
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
	RegisterFailHandler(Fail)
	RunSpecs(t, "isLabradarCsvFile")
}

func TestFilenameForSeries(t *testing.T) {
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
			if got := fs.FilenameForSeries(tt.args.labradarDirectory, tt.args.seriesNumber); got != tt.want {
				t.Errorf("FilenameForSeries() = %v, want %v", got, tt.want)
			}
		})
	}
}
