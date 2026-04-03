package shotsession

import (
	"fmt"
	"os"

	"github.com/muktihari/fit/profile/untyped/mesgnum"
	"github.com/muktihari/fit/proto"
)

const (
	MinSpeedFieldNum          = 0
	MaxSpeedFieldNum          = 1
	AvgSpeedFileNum           = 2
	ShotCountFieldNum         = 3
	ProjectileTypeFieldNum    = 4
	GrainWeightFieldNum       = 5
	StandardDeviationFieldNum = 6
	TimestampFieldNum         = 253
)

type ShotSession struct {
	MinSpeed          float64
	MaxSpeed          float64
	AvgSpeed          float64
	ShotCount         int
	ProjectileType    string
	GrainWeight       float64
	StandardDeviation float64
	Timestamp         int64
}

func ParseShotSession(m proto.Message) *ShotSession {

	if m.Num != mesgnum.ChronoShotSession {
		return nil
	}
	fmt.Fprintf(os.Stdout, "   ShotSession (%d)\n", m.Num)
	if len(m.Fields) > 0 {
		fmt.Fprintf(os.Stdout, "      Fields (%d)\n", len(m.Fields))
		for i, f := range m.Fields {
			fmt.Fprintf(os.Stdout, "        ")
			fmt.Fprintf(os.Stdout, "%d: %s (%d)\n", i, f.Name, f.Num)
		}
	} else {
		fmt.Fprintf(os.Stdout, "No fields available.\n")
	}

	return &ShotSession{}
}
