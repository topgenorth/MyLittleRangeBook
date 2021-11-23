package labradar

import . "github.com/onsi/ginkgo"
import . "github.com/onsi/gomega"
import "testing"

var _ = Describe("isLabradarCsvFile", func() {
	var (
		valid    string
		invalid1 string
		invalid2 string
	)

	BeforeEach(func() {
		valid = "/tmp/data/LBR/SR0001/SR0001 Report.csv"
		invalid1 = "/tmp/data/LBR/SR0001/SR0001 Report.txt"
		invalid2 = "/tmp/data/LBR/SR0001/0001 Report.csv"
	})

	Describe("Check if Labradar CSV ", func() {
		Context("Has correct prefix and extension", func() {
			It("should pass.", func() {
				Expect(isLabradarCsvFile(valid)).To(BeTrue())
			})
		})
		Context("Has invalid extension", func() {
			It("should not pass", func() {
				Expect(isLabradarCsvFile(invalid1)).To(BeFalse())
			})
		})
		Context("Does not start with SR", func() {
			It("should not pass", func() {
				Expect(isLabradarCsvFile(invalid2)).To(BeFalse())
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
			if got := FilenameForSeries(tt.args.labradarDirectory, tt.args.seriesNumber); got != tt.want {
				t.Errorf("FilenameForSeries() = %v, want %v", got, tt.want)
			}
		})
	}
}
