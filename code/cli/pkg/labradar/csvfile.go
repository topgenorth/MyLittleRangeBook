package labradar

import (
	"bufio"
	"fmt"
	"github.com/carolynvs/aferox"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"os"
	"path"
	"path/filepath"
	"strings"
)

type DataFiles struct {
	InputDir string
	Files    []CsvFile
}

func LoadDataFiles(c *config.Config, inputDir string) DataFiles {
	return DataFiles{
		inputDir,
		getLabradarCsvFiles(c, inputDir),
	}
}

func LoadCsv(c *config.Config, inputDir string, seriesNumber int) *CsvFile {
	f := loadCsvFile(c.FileSystem, FilenameForSeries(inputDir, seriesNumber))
	return f
}

type CsvFile struct {
	*Series
	InputFile string
	Error     error
}

func (f CsvFile) String() string {
	return f.InputFile
}

func getLabradarCsvFiles(c *config.Config, inputDir string) []CsvFile {
	var files []CsvFile
	for _, filename := range getCsvFilenamesInDirectory(inputDir) {
		f := loadCsvFile(c.FileSystem, filename)
		if f.Error == nil {
			files = append(files, *f)
		}
	}
	return files
}

func loadCsvFile(fs aferox.Aferox, filename string) *CsvFile {
	f, err := fs.Open(filename)
	if err != nil {
		return &CsvFile{
			nil,
			filename,
			err,
		}
	}
	defer closeFile(f)

	sb := NewSeriesBuilder()
	s := bufio.NewScanner(f)
	var lineNumber = 0
	for s.Scan() {
		ld := NewLineOfData(lineNumber, s.Text())
		sb.ParseLine(ld)
		lineNumber++
	}

	return &CsvFile{
		Series:    sb.Series,
		InputFile: filename,
		Error:     nil,
	}
}

func closeFile(f afero.File) {
	err := f.Close()
	if err != nil {
		fmt.Println(err)
	}
}
func isLabradarCsvFile(path string) bool {
	b := strings.ToLower(filepath.Base(path))
	ext := filepath.Ext(b)
	if ext == ".csv" {
		sr := b[0:2]
		if sr == "sr" {
			return true
		}
	}

	return false
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
func outputFileNameFor(seriesNumber int, outputDir string) string {
	stub := fmt.Sprintf("%04d", seriesNumber)
	filename := fmt.Sprintf("%s.json", stub)
	return path.Join(outputDir, filename)
}
func getCsvFilenamesInDirectory(inputDir string) []string {
	// TODO [TO20211122] Should verify that labradarDirectory is actually a directory.
	var filenames []string

	// TODO [TO20211122] Can't really mock out filepath.Walk.
	err := filepath.Walk(inputDir, func(path string, info os.FileInfo, err error) error {
		if !info.IsDir() && isLabradarCsvFile(path) {
			filenames = append(filenames, path)
		}
		return nil
	})
	if err != nil {
		logrus.Errorf("Could not list files in the directory %s. %v", inputDir, err)
		return nil
	}

	return filenames
}
