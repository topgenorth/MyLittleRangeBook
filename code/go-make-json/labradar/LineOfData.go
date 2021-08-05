package labradar

import (
	"strconv"
	"strings"
)

type LineOfData struct {
	LineNumber int `json:"lineNumber"`
	Raw   string `json:"raw"`
	Value string `json:"value"`
}

func CreateLine(linenumber int, s string) *LineOfData {
	return &LineOfData{
		LineNumber: linenumber,
		Raw:        s,
		Value:      fixupLabradarLine(s),
	}
}

func (ld *LineOfData) GetString() string {
	parts := strings.Split(ld.Value, ";")
	return parts[1]
}

func (ld *LineOfData) GetInt() int {
	parts := strings.Split(ld.Value, ";")
	i, _ := strconv.Atoi(parts[1])
	return i
}

func (ld *LineOfData) GetDateAndTime() (string, string) {
	parts := strings.Split(ld.Value, ";")
	l := len(parts)
	return parts[l-3], parts[l-2]
}
