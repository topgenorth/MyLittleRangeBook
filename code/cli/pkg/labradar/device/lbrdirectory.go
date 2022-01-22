package device

import (
	"fmt"
	"github.com/spf13/afero"
	"os"
)

// LbrDirectory represents a string that is the full path to the LBR directory holding Labradar files.
type LbrDirectory string

func (d LbrDirectory) String() string {
	return string(d)
}

// GetLabradarMarkerFile will inspect all the files in the LbrDirectory and attempt to find an
// *.LID file that all LBR data folders seem to have.
func (d LbrDirectory) GetLabradarMarkerFile(af *afero.Afero) (os.FileInfo, error) {
	f, err := findTheLabradarMarkerFile(d.String(), af)
	if err != nil {
		return nil, fmt.Errorf(" '%s' does not seem to be an LBR directory", d.String())
	}
	if f == nil {
		return nil, fmt.Errorf("does not seem to be a LBR directory '%s'", d.String())
	}

	return f, nil
}

// GetSerialNumber is a convenience method that will try to infer the serial number of a Labradar device based on it's
// LBR directory.  Will return an "" if is not possible to do so.
func (d LbrDirectory) GetSerialNumber(af *afero.Afero) string {
	f, err := d.GetLabradarMarkerFile(af)
	if err != nil {
		return ""
	}

	fn, err := parseDeviceIdFromFilename(f.Name())
	if err != nil {
		return ""
	}

	return getDeviceId(fn)
}
