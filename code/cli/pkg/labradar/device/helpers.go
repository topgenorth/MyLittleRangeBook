package device

import (
	"fmt"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/math"
	"os"
	"path/filepath"
	"strings"
)

// findTheLabradarMarkerFile will inspect all the files in a given directory and attempt to find an
// *.LID file that all the LBR data folders seem to have.
func findTheLabradarMarkerFile(path string, af *afero.Afero) (os.FileInfo, error) {
	b, err := af.Exists(path)
	if err != nil || !b {
		return nil, fmt.Errorf("could not determine if '%s' is Labradar directory: %w", path, err)
	}

	d, err := af.Open(path)
	if err != nil {
		return nil, fmt.Errorf("could not determine if '%s' is Labradar directory: %w", path, err)
	}
	files, err := d.Readdir(0)
	if err != nil {
		return nil, fmt.Errorf("could not determine if '%s' is Labradar directory: %w", path, err)
	}

	for _, f := range files {
		if f.IsDir() {
			break
		}

		if f.Size() != 0 {
			break
		}

		if looksLikeTheLabradarMarkerFile(f.Name()) {
			return f, nil
		}
	}

	return nil, nil
}

// getDeviceId will parse a file name and extract the name of the device, which looks like LBR-0013797.
func getDeviceId(filename string) string {
	serialNumber, err := parseDeviceIdFromFilename(filename)
	if err != nil {
		return "LBR-0000000"
	}
	return fmt.Sprintf("%s-%s", filename[0:3], serialNumber)
}

// parseDeviceIdFromFilename will pull out the device ID from a filename. An error will be thrown if the
// device ID is not a number.
func parseDeviceIdFromFilename(filename string) (string, error) {
	// TODO [TO20220119] Needs unit tests
	serialNumber := filename[3:10]

	if !math.IsNumericOnly(serialNumber) {
		return "", fmt.Errorf("could not parse a numeric value for the device id from %s", filename)
	}

	return serialNumber, nil
}

// looksLikeTheLabradarMarkerFile will inspect a filename and try to infer if the file is the Labradar .LID file.
// The file looks like  LBR0013797201909141617.LID that is a zero-byte file.
func looksLikeTheLabradarMarkerFile(filename string) bool {

	if !strings.HasPrefix(filename, "LBR") {
		return false
	}
	if filepath.Ext(filename) != ".LID" {
		return false
	}

	if len(filename) != 26 {
		return false
	}

	_, err := parseDeviceIdFromFilename(filename)
	if err != nil {
		return false
	}
	return true
}

func tryParseLbrDirectory(path string, af *afero.Afero) (LbrDirectory, error) {
	emptyLbrDirectory := LbrDirectory("")

	// TODO [TO20220120] Clean up some of the repetitiveness around the validation.

	if len(path) == 0 {
		return emptyLbrDirectory, fmt.Errorf("must provide a valid path to the LBR directory")
	}
	isDir, err := af.IsDir(path)
	if err != nil {
		if isDir {
			return emptyLbrDirectory, fmt.Errorf("could not determine if '%s' is a directory", path)
		} else {
			return emptyLbrDirectory, fmt.Errorf("%s does not exist", path)
		}
	}
	if !isDir {
		return emptyLbrDirectory, fmt.Errorf("'%s' is not a directory", path)
	}

	return LbrDirectory(path), nil
}
