package labradar

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/util"
	"path/filepath"
)

// SeriesNumber is a custom type that represents a Labradar series.
type SeriesNumber int

func (t *SeriesNumber) String() string {
	return fmt.Sprintf("SR" + util.PadLeft(t.Int(), 4))
}
func (t *SeriesNumber) Int() int64 {
	return int64(*t)
}
func (t *SeriesNumber) ReportCsv() string {
	return fmt.Sprintf("%s Report.csv", t.String())
}
func (t *SeriesNumber) LbrName() string {
	return fmt.Sprintf("%s.lbr", t.String())
}

func (t *SeriesNumber) pathToReportCsvOn(d *DeviceDirectory) string {
	return filepath.Join(d.directory.String(), t.String(), t.ReportCsv())
}

func (t *SeriesNumber) ExistsOn(d *DeviceDirectory) bool {
	exists, err := afero.Exists(d.af, t.pathToReportCsvOn(d))
	if err != nil {
		logrus.WithError(err).Warningf("There was a problem trying determine if the series %s is on the device %s.", t.String(), d.String())
		return false
	}
	return exists
}
