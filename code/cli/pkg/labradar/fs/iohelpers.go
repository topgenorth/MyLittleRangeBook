// Package fs encapsulates all of the code for manipulating the files created by a Labradar.
package fs

import (
	"fmt"
	"github.com/carolynvs/aferox"
	"github.com/pkg/errors"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/math"
	"os"
	"path"
	"path/filepath"
	"sort"
	"strings"
)

// DeleteFile will try and delete the specific file, returning true if it can.
func DeleteFile(s string, c *config.Config) bool {
	exists, err := c.FileSystem.Exists(s)
	if err != nil {
		return false
	}
	if exists {
		err := os.Remove(s)
		// TODO [TO20220105] Log a warning.
		if err != nil {
			return false
		}
	}
	return true
}

func CloseFile(f afero.File) {
	err := f.Close()
	if err != nil {
		logrus.Error(err)
	}
}

// FilenameForSeries Given the number of a series and the root directory of the Labradar files, infer the filename of the Labradar
// CSV file that holds the results of the series.
func FilenameForSeries(labradarRootDirectory string, seriesNumber int) string {
	stub := fmt.Sprintf("%04d", seriesNumber)
	//goland:noinspection SpellCheckingInspection
	subdir := fmt.Sprintf("SR%s", stub)
	filename := fmt.Sprintf("SR%s Report.csv", stub)
	p := path.Join(labradarRootDirectory, subdir, filename)
	return p
}

// ListLabradarFiles will return an array of all the full filenames of Labradar CSV files in the given
// directory of Labradar files.
func ListLabradarFiles(inputDir string, fs aferox.Aferox) []string {
	result := make([]string, 0)

	// [TO20220115] Show a list of all the things in the inputDir.
	labradarDir, err := fs.Open(inputDir)
	if err != nil {
		logrus.Warnf("Could not list the contents of %s: %v", inputDir, err)
		return result
	}
	subdirs, err := labradarDir.Readdir(0)
	if err != nil {
		logrus.Warnf("problem finding subdirs in %s: %v", inputDir, err)
		return result
	}

	for _, subdir := range subdirs {
		s := subdir.Name()
		if isLabradarSeriesName(s) {
			d := filepath.Join(inputDir, s, fmt.Sprintf("%s Report.csv", s))
			if fileExists(d) {
				result = append(result, d)
			}
		}
	}

	sort.Strings(result)

	return result
}

func LoadCsv(filename string, fs aferox.Aferox) *CsvFile {
	csv := &CsvFile{
		contents:  make([]byte, 0),
		InputFile: filename,
		Error:     nil,
	}

	f, err := fs.Open(filename)
	defer CloseFile(f)
	if err != nil {
		csv.Error = err
		return csv
	}
	fi, err := f.Stat()
	if err != nil {
		csv.Error = err
		return csv
	}

	filesize := int(fi.Size())
	csv.contents = make([]byte, filesize)

	_, err = f.Read(csv.contents)
	if err != nil {
		csv.Error = err
	}

	return csv
}

// isLabradarSeriesName will return true if the directory name looks like a valid Labradar series directory.
// This is a case sensitive comparison.
func isLabradarSeriesName(dirName string) bool {
	if len(dirName) != 6 {
		return false
	}

	if dirName[0:2] != "SR" {
		return false
	}

	number := dirName[2:]
	if !math.IsNumericOnly(number) {
		return false
	}

	return true
}

// isLabradarCsvFile will perform some basic checking to see if a given filename could be that of a Labradar CSV file.
// This is a case sensitive comparison.  Sample name: SR0001 Report.csv
func isLabradarCsvFile(filenameWithExtension string) bool {

	if len(filenameWithExtension) != 17 {
		return false
	}

	b := filepath.Base(filenameWithExtension)
	ext := filepath.Ext(b)

	// Check extension
	if ext != ".csv" {
		return false
	}

	name := strings.Split(b, ".")[0]

	// Check length of the filename
	if len(name) != 13 {
		return false
	}

	// Must end with ` Report`
	if !strings.HasSuffix(name, " Report") {
		return false
	}

	// Must start with a valid Labradar OldSeries name
	return isLabradarSeriesName(name[0:6])

}

func fileExists(filename string) bool {
	if _, err := os.Stat(filename); err == nil {
		// path/to/whatever exists
		return true
	} else if errors.Is(err, os.ErrNotExist) {
		// path/to/whatever does *not* exist
		return false

	} else {
		// Schrodinger: file may or may not exist. See err for details.

		// Therefore, do *NOT* use !os.IsNotExist(err) to test for file existence
		logrus.Errorf("Assuming the file %s does not exist. %v", filename, err)
		return false
	}

	return true
}
