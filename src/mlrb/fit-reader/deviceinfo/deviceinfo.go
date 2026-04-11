package deviceinfo

import (
	"github.com/muktihari/fit/profile/untyped/mesgnum"
	"github.com/muktihari/fit/proto"
)

const (
	ManufacturerFieldNum    = 2
	SerialNumberFieldNum    = 3
	ProductFieldNum         = 4
	SoftwareVersionFieldNum = 5
	TimestampFieldNum       = 253
)

func ParseSerialNumber(m proto.Message) uint32 {
	if m.Num != mesgnum.DeviceInfo {
		return 0
	}

	f := m.FieldByNum(SerialNumberFieldNum)
	if f == nil {
		return 0
	}
	return f.Value.Uint32z()
}
