package labradar

import (
	"opgenorth.net/labradar/util"
	"strconv"
	"strings"
)

type LineOfData struct {
	LineNumber int
	RawValue   string
	CleanValue string
}

func CreateLine(linenumber int, s string) *LineOfData {
	return &LineOfData{
		LineNumber: linenumber,
		RawValue:   s,
		CleanValue: util.FixupLabradarLine(s),
	}
}

func (ld *LineOfData) GetString() string {
	parts := strings.Split(ld.CleanValue, ";")
	return parts[1]
}

func (ld *LineOfData) GetInt() int {
	parts := strings.Split(ld.CleanValue, ";")
	i, _ := strconv.Atoi(parts[1])
	return i
}

func (ld *LineOfData) GetDateAndTime() (string, string) {
	parts := strings.Split(ld.CleanValue, ";")
	l := len(parts)
	return parts[l-3], parts[l-2]
}
