package labradar

import (
	"opgenorth.net/labradar/pkg/context"
	"testing"
	"time"
)

func Test_initLabradarStruct(t *testing.T) {
	loc, _ := time.LoadLocation(context.DefaultTimeZone)
	ls := initDevice(42, loc)

	if ls.SeriesName != "SR0042" {
		t.Errorf("ls.SeriesName = %s; wanted SR0042.", ls.SeriesName)
	}

}
