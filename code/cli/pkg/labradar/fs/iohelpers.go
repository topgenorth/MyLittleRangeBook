package fs

import (
	"fmt"
	"github.com/carolynvs/aferox"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"os"
	"path"
)

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

/*
// LoadDataFiles will attempt load and de-serialize the CSV files for a given Labradar instance.
func LoadDataFiles(c *config.Config, inputDir string) *LbrFiles {
	dir, err := os.Stat(inputDir)

	f := &LbrFiles{
		inputDir,
		make([]*CsvFile, 0),
	}
	if err != nil {
		return f
	}

	if !dir.IsDir() {
		return f
	}

	f.Files = getLabradarCsvFiles(c, inputDir)
	return f
}

// getLabradarCsvFiles will read all the Labradar CSV files from the given directory.
func getLabradarCsvFiles(c *config.Config, inputDir string) []*CsvFile {
	var files []*CsvFile
	for _, filename := range getCsvFilenamesInDirectory(inputDir) {
		logrus.Debugf("Trying to read the file %s.", filename)
		f := loadCsvInternal(c.FileSystem, filename)
		if f.Error == nil {
			files = append(files, f)
		}
	}
	return files
}
*/

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
