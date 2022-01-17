package labradar

import (
	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"
	"reflect"
	"testing"
	"time"
)

func TestNewLineOfData(t *testing.T) {
	RegisterFailHandler(Fail)
	RunSpecs(t, "Lines of text from the Labradar CSV file")
}

var _ = Describe("Lines of text from the Labradar CSV file", func() {
	var (
		lineOfData *LineOfData
	)
	Context("parse the 1st line", func() {

		It("should have the OldDevice ID", func() {
			lineOfData = NewLineOfData(1, "OldDevice ID;LBR-0013797;;\u0000                                     \n")
			Expect(lineOfData.Value).To(BeEquivalentTo("OldDevice ID;LBR-0013797;;"))
		})
	})

	Context("parse the 3rd line", func() {

		It("should have the OldSeries Number", func() {
			lineOfData = NewLineOfData(3, "\u0000Series No;0001;;\u0000                                            \n")
			Expect(lineOfData.Value).To(BeEquivalentTo("OldSeries No;0001;;"))
		})
	})

	Context("parse the 4th line", func() {

		It("should have the total Number of shots", func() {
			lineOfData = NewLineOfData(4, "\u0000Total Number of shots;0009;;\u0000                                \n")
			Expect(lineOfData.Value).To(BeEquivalentTo("Total Number of shots;0009;;"))
		})
	})

})

func Test_parseDateAndTime(t *testing.T) {
	type args struct {
		d string
		t string
	}
	tests := []struct {
		name string
		args args
		want time.Time
	}{
		{"Should handle an afternoon hour in the 24 hour clock.",
			args{"07-30-2020", "19:05:02"},
			time.Date(2020, 7, 30, 19, 05, 02, 0, time.UTC),
		},
		{"Should handle an morning hour in the 24 hour clock.",
			args{"07-30-2020", "11:05:02"},
			time.Date(2020, 7, 30, 11, 05, 02, 0, time.UTC),
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := parseDateAndTime(tt.args.d, tt.args.t); !reflect.DeepEqual(got, tt.want) {
				t.Errorf("parseDateAndTime() = %v, want %v", got, tt.want)
			}
		})
	}
}
