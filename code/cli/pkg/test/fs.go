package test

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"path/filepath"
)

const lbrDir = "LBR/"
const markerFile = "LBR0013797201909141617.LID"
const testFiles = "../../data/LBR/"

var testFs = afero.NewMemMapFs()
var appFs = afero.NewOsFs()

// InitLabradarFilesystemForTest will create some Labradar test data
func InitLabradarFilesystemForTest() afero.Fs {

	err := testFs.Mkdir(lbrDir, 0644)
	if err != nil {
		logrus.WithError(err).Error("Could not create the Labradar directory for the tests.")
		return testFs
	}

	// [TO20220320] Write a zero byte file for the marker file.
	err = afero.WriteFile(testFs, markerFile, []byte{}, 0644)
	if err != nil {
		logrus.WithError(err).Error("Could not create the LID marker file.")
	}

	_ = putLabradarSeriesIntoMemoryForTests()

	return testFs
}

func putLabradarSeriesIntoMemoryForTests() error {
	// TODO [TO20220320] Clean this up.
	testSeries := make([]string, 6)
	testSeries[0] = "SR0001"
	testSeries[1] = "SR0002"
	testSeries[2] = "SR0003"
	testSeries[3] = "SR0004"
	testSeries[4] = "SR0005"
	testSeries[5] = "SR0006"

	var err error
	for _, name := range testSeries {
		err = copySeriesFilesForTest(name)
		if err != nil {
			logrus.WithError(err).Warnf("Could not copy the series %s to the memory FS.", name)
		}
	}

	if err != nil {
		return fmt.Errorf("There was a problem trying to setup the Labrader series for the test.")
	}
	return nil
}

func copySeriesFilesForTest(name string) error {
	// [TO20220320] Copy each of the SRxxxx directories from the disk to the in memory filesystem.
	err := testFs.Mkdir(filepath.Join(lbrDir, name), 0644)
	if err != nil {
		return err
	}

	// TODO [TO20220320] How do we run these two in parallel?
	err = copyLbrFileForSeries(name)
	if err != nil {
		return err
	}

	err = copyReportCsvFile(name)
	if err != nil {
		return err
	}

	return nil
}

func copyReportCsvFile(srName string) error {
	// [TO20220320] Copy the *Report.csv file
	csvName := srName + " Report.csv"
	csvInFilename := filepath.Join(testFiles, srName, csvName)
	csvBytes, err := afero.ReadFile(appFs, csvInFilename)
	if err != nil {
		return err
	}
	csvOutFilename := filepath.Join(lbrDir, srName, csvName)
	err = afero.WriteFile(testFs, csvOutFilename, csvBytes, 0644)
	if err != nil {
		return err
	}
	return nil
}

func copyLbrFileForSeries(srName string) error {
	// [TO20220320] Copy the LBR file first
	lbrName := srName + ".lbr"
	lbrInFilename := filepath.Join(testFiles, srName, lbrName)
	inBytes, err := afero.ReadFile(appFs, lbrInFilename)
	if err != nil {
		return err
	}
	lbrOutFilename := filepath.Join(lbrDir, srName, lbrDir)
	err = afero.WriteFile(testFs, lbrOutFilename, inBytes, 0644)
	if err != nil {
		return err
	}

	return nil
}
