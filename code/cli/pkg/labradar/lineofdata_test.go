package labradar

import (
	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"
	"testing"
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

		It("should have the Device ID", func() {
			lineOfData = NewLineOfData(1, "Device ID;LBR-0013797;;\u0000                                     \n")
			Expect(lineOfData.Value).To(BeEquivalentTo("Device ID;LBR-0013797;;"))
		})
	})

	Context("parse the 3rd line", func() {

		It("should have the Series number", func() {
			lineOfData = NewLineOfData(3, "\u0000Series No;0001;;\u0000                                            \n")
			Expect(lineOfData.Value).To(BeEquivalentTo("Series No;0001;;"))
		})
	})

	Context("parse the 4th line", func() {

		It("should have the total number of shots", func() {
			lineOfData = NewLineOfData(4, "\u0000Total number of shots;0009;;\u0000                                \n")
			Expect(lineOfData.Value).To(BeEquivalentTo("Total number of shots;0009;;"))
		})
	})

})

