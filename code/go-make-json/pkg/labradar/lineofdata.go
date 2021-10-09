package labradar

import (
	"strconv"
	"strings"
)

type LineOfData struct {
	LineNumber int    `json:"lineNumber"`
	Raw        string `json:"raw"`
	Value      string `json:"value"`
}

func newLineOfData(linenumber int, s string) *LineOfData {
	return &LineOfData{
		LineNumber: linenumber,
		Raw:        s,
		Value:      fixupLabradarLine(s),
	}
}

func (ld *LineOfData) getStringValue() string {
	parts := strings.Split(ld.Value, ";")
	if len(parts) < 2 {
		return ""
	}
	return parts[1]
}

func (ld *LineOfData) getIntValue() int {
	parts := strings.Split(ld.Value, ";")
	if len(parts) < 2 {
		return -1
	}
	i, _ := strconv.Atoi(parts[1])
	return i
}

func (ld *LineOfData) getDateAndTime() (string, string) {
	parts := strings.Split(ld.Value, ";")
	l := len(parts)
	if l == 1 {
		return "", ""
	}
	return parts[l-3], parts[l-2]
}
