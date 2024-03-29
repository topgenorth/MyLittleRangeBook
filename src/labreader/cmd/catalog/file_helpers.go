package catalog

import (
	"github.com/pkg/errors"
	"github.com/spf13/afero"
	constants "opgenorth.net/tomutil"
	"opgenorth.net/tomutil/timeprovider"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"time"
)

// destinationDirectory will return a string that is the path to the directory that the file will be moved to.
func destinationDirectory(clv commandLineValues) string {
	var part string
	path := clv.Rifle

	if constants.UnknownStr != clv.Cartridge {
		part = strings.ReplaceAll(clv.Cartridge, ".", "")
		path = filepath.Join(path, strings.ToLower(part))
	}

	b := clv.getBullet().String()
	if "" != b {
		part = strings.ReplaceAll(clv.getBullet().String(), ".", "")
		path = filepath.Join(path, part)
	}

	p := clv.getPowder().String()
	if "" != p {

		path = filepath.Join(path, p)
	}

	if !isValidPath(path) {
		// TODO [TO20231210] Need a better way to handle this.
		return ""
	}

	path = filepath.Join(clv.CatalogFolder, path)
	return path
}

func timestampDestinationFile(filePath string, timeProvider *timeprovider.TimeProvider) string {
	var destinationFilename string
	prefix := timeProvider.String() + "-"
	filename := filepath.Base(filePath)

	if startsWithyyyyMMdd(filename) {
		// [TO20231212] Verify that this is a valid date and not in the future.
		datePart, err := parseDateyyyyMMdd(filename)
		if err != nil {
			// [TO20231212] This is not a valid date; append our prefix.
			destinationFilename = prefix + filename
		} else if datePart.Before(timeProvider.Now()) {
			// [TO20231212] This starts with a valid date and time; do not append our prefix.
			destinationFilename = filename
		} else {
			// [TO20231212] This is a date in the future; append our prefix.
			destinationFilename = prefix + filename
		}
	} else {
		// [TO20231212] doesn't start with a string that could be a date; append our prefix
		destinationFilename = prefix + filename
	}

	return destinationFilename

}

func moveFile(afs *afero.Afero, source string, destination string) error {
	dir := filepath.Dir(destination)
	err := afs.MkdirAll(dir, os.ModePerm)
	if err != nil {
		return err
	}

	err = afs.Rename(source, destination)
	if err != nil {
		return err
	}
	return nil
}
func startsWithyyyyMMdd(filename string) bool {
	// Regular expression pattern for yyyyMMdd date format
	yyyyMMddPattern := `^\d{8}` // This matches 8 consecutive digits at the beginning of the string

	// Compile the regular expression pattern
	regex := regexp.MustCompile(yyyyMMddPattern)

	// Check if the filename matches the yyyyMMdd pattern at the beginning
	return regex.MatchString(filename)
}

func parseDateyyyyMMdd(dateString string) (time.Time, error) {
	if len(dateString) > 8 {
		dateString = dateString[:8]
	}
	parsedDate, err := time.Parse(dateLayoutForFilePrefix, dateString)
	if err != nil {
		return time.Time{}, errors.Wrap(err, "not a date in the format "+dateLayoutForFilePrefix)
	}
	return parsedDate, nil
}
