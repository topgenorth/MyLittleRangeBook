package labradar

import (
	"fmt"
	"github.com/carolynvs/aferox"
	"opgenorth.net/mylittlerangebook/pkg/context"
	"opgenorth.net/mylittlerangebook/pkg/labradar/fs"
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"os"
	"path/filepath"
	"strconv"
	"strings"
)

// Device holds the details about a specific Labradar device.
type Device struct {
	//DeviceId is a unique ID that is assigned to each Labradar device.
	DeviceId string `json:"deviceId"`
	// Timezone is the timezone that the clock on the Labradar is set to.
	TimeZone string `json:"timezone"`
	// Directory is the name of the folder that holds the Labradar files.
	Directory string `json:"directory"`
	af        aferox.Aferox
}

func (d Device) String() string {
	return d.DeviceId
}

func NewDevice(path string, ctx *context.AppContext) (*Device, error) {

	file, err := findTheLabradarMarkerFile(path, ctx.FileSystem)
	if err != nil {
		return nil, err
	}
	if file == nil {
		return nil, fmt.Errorf("does not seem to be a Labradar directory '%s'", path)
	}

	return &Device{
		DeviceId:  getDeviceId(file.Name()),
		TimeZone:  ctx.Timezone,
		Directory: path,
		af:        ctx.FileSystem}, nil
}

func (d Device) LoadSeries(seriesNumber int) (*series.LabradarSeries, error) {
	filename := fs.FilenameForSeries(d.Directory, seriesNumber)
	builders, err := fs.FromCsvFile(filename, d.af)

	if err != nil {
		e := series.SeriesError{
			Msg:    fmt.Sprintf("could not load the series %d from  device %s (%s): %w", seriesNumber, d.DeviceId, d.Directory, err),
			Number: seriesNumber,
		}
		return nil, e
	}

	s := series.New(builders...)
	return s, nil

}

// WithDevice will initialize the Labradar device id and
//func WithDevice(device *Device, tz *time.Location) series.LabradarSeriesMutatorFunc {
//	return func(s *series.LabradarSeries) {
//		s.Labradar = device
//		s.Labradar.TimeZone = tz.String()
//	}
//}

func findTheLabradarMarkerFile(path string, af aferox.Aferox) (os.FileInfo, error) {
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

func getDeviceId(name string) string {
	return fmt.Sprintf("%s-%s", name[0:3], name[3:6])
}

func looksLikeTheLabradarMarkerFile(name string) bool {
	// LBR0013797201909141617.LID
	// LBR-0013797

	if !strings.HasSuffix(name, "LBR") {
		return false
	}
	if filepath.Ext(name) != ".LID" {
		return false
	}

	if len(name) < 10 {
		return false
	}
	serialNumber := name[3:6]
	_, err := strconv.Atoi(serialNumber)
	if err != nil {
		return false
	}
	return true
}
