package test

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/fs"
	"os"
	"path/filepath"
	"runtime"
	"strings"
)

// LBRDirectory is a variable that will hold either '\LBR' or '/LBR', depending on the operating system.
var LBRDirectory string

// testFileDirectory holds the path to the LBR directory that holds the current/existing data files from Labradar.
var testFileDirectory string

// LIDFile is the filename of a file that the Labradar will create on the SD card.  No idea what it is for.
const LIDFile = "LBR0013797201909141617.LID"

// AferoTestFs is a reference to the in-memory filesystem used for testing.
var AferoTestFs = afero.NewMemMapFs()

// AferoOsFs is a reference to the actually disk filesystem.
var AferoOsFs = afero.NewOsFs()

func init() {
	LBRDirectory = string(os.PathSeparator) + "LBR"
	testFileDirectory = filepath.Join("..", "..", "data", "LBR")
}

// InitLabradarFilesystemForTest will create some Labradar test data.  Returns a reference to the in-memory filesystem.
func InitLabradarFilesystemForTest() afero.Fs {

	err := AferoTestFs.Mkdir("/"+LBRDirectory, 0644)
	if err != nil {
		logrus.WithError(err).Error("Could not create the Labradar directory for the tests.")
		return AferoTestFs
	}

	// [TO20220320] Write a zero byte file for the marker file.
	err = afero.WriteFile(AferoTestFs, filepath.Join(LBRDirectory, LIDFile), []byte{}, 0644)
	if err != nil {
		logrus.WithError(err).Error("Could not create the LID marker file.")
	}
	err = putLabradarSeriesIntoMemoryForTests()
	if err != nil {
		logrus.WithError(err).Error("There was a problem setting up MemFs like a Labradar.")
	}
	return AferoTestFs
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

		return fmt.Errorf("There was a problem trying to setup the Labrader series for the test. %w", err)
	}
	return nil
}

func copySeriesFilesForTest(name string) error {
	// [TO20220320] Copy each of the SRxxxx directories from the disk to the in memory filesystem.
	err := AferoTestFs.Mkdir(filepath.Join(LBRDirectory, name), 0644)
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
	dataDir, err := getTestDataDir()
	if err != nil {
		return err
	}

	csvName := srName + " Report.csv"
	srcFile := filepath.Join(dataDir, srName, csvName)
	csvBytes, err := afero.ReadFile(AferoOsFs, srcFile)

	if err != nil {
		return err
	}
	dstFile := filepath.Join(LBRDirectory, srName, csvName)
	err = afero.WriteFile(AferoTestFs, dstFile, csvBytes, 0644)
	if err != nil {
		return err
	}

	logrus.Tracef("Copied the file %s to %s.", srcFile, dstFile)
	return nil
}

func getTestDataDir() (string, error) {
	// C:\Users\tom.opgenorth\code\MyLittleRangeBook\data\LBR\SR0001\SR0001.lbr
	// C:\Users\tom.opgenorth\code\MyLittleRangeBook\code\cli\data\LBR\SR0001\SR0001.lbr
	// C:Users\tom.opgenorth\code\MyLittleRangeBook\data\LBR\SR0001\SR0001.lbr
	wd, err := fs.CurrentWd()
	if err != nil {
		return "", err
	}

	parts := strings.Split(wd, string(filepath.Separator))
	if runtime.GOOS == "windows" {
		// [TO20220323] A bit of a hack - if this is windows then we have to add the \ to the drive letter.
		parts[0] = parts[0] + "\\"
	}

	for i := range parts {
		if parts[i] == "MyLittleRangeBook" {
			path := filepath.Join(parts[0 : i+1]...)
			return filepath.Join(path, "data", "LBR"), nil
		}
	}

	return "", fmt.Errorf("This does not seem to be a MyLittleRangeBook directory: %s.", wd)
}
func copyLbrFileForSeries(srName string) error {
	dataDir, err := getTestDataDir()
	if err != nil {
		return err
	}
	lbrName := srName + ".lbr"
	srcFile := filepath.Join(dataDir, srName, lbrName)

	inBytes, err := afero.ReadFile(AferoOsFs, srcFile)
	if err != nil {

		return err
	}
	dstFile := filepath.Join(LBRDirectory, srName, LBRDirectory)
	err = afero.WriteFile(AferoTestFs, dstFile, inBytes, 0644)
	if err != nil {
		return err
	}

	logrus.Tracef("Copied the file %s to %s.", srcFile, dstFile)

	return nil
}
