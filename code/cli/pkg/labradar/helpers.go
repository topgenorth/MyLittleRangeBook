package labradar

import (
	"fmt"
	"github.com/spf13/afero"
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
		// TODO [TO20220122] Look at this icky arrowhead!
		if !f.IsDir() {
			if f.Size() == 0 {
				if looksLikeTheLabradarMarkerFile(f.Name()) {
					return f, nil
				}
			}
		}
	}

	return nil, nil
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
