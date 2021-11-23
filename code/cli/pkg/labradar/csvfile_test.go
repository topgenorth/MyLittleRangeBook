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
				Expect(true, isLabradarCsvFile(valid))
			})
		})

		Context("Has invalid extension", func() {
			It("should not pass", func() {
				Expect(false, isLabradarCsvFile(invalid1))
			})
		})
		Context("Does not start with SR", func() {
			It("should not pass", func() {
				Expect(false, isLabradarCsvFile(invalid2))
			})
		})
	})
})

func Test_isLabradarCsvFile(t *testing.T) {
	RegisterFailHandler(Fail)
	RunSpecs(t, "isLabradarCsvFile")
}
