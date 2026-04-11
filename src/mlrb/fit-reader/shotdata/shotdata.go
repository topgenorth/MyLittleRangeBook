package shotdata

import (
	"github.com/muktihari/fit/profile/untyped/mesgnum"
	"github.com/muktihari/fit/proto"
)

const (
	ShotSpeedFieldNum  = 0
	ShotNumberFieldNum = 1
	TimestampFieldNum  = 253
)

type ShotData struct {
	Speed     float64
	Number    int
	Timestamp int64
}

func ParseShotData(m proto.Message) *ShotData {

	if m.Num != mesgnum.ChronoShotData {
		return nil
	}
	
	return &ShotData{}
}
