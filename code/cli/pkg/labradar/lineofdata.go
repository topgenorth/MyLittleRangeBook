package labradar

import (
	"opgenorth.net/mylittlerangebook/pkg"
	"sort"
	"strconv"
	"strings"
)

type LineOfData struct {
	LineNumber int    `json:"lineNumber"`
	Raw        string `json:"raw"`
	Value      string `json:"value"`
}

func NewLineOfData(linenumber int, s string) *LineOfData {
	return &LineOfData{
		LineNumber: linenumber,
		Raw:        s,
		Value:      fixupLabradarLine(s),
	}
}

func (l LineOfData) String() string {
	return l.Value
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

func fixupLabradarLine(line string) string {
	parts := strings.Split(strings.TrimSpace(line), pkg.UnicodeNUL)

	switch lengthOfParts := len(parts); lengthOfParts {
	case 2:
		// The string either started with a NUL or ended with a NUL
		if len(parts[0]) == 0 {
			return parts[1]
		}
		if len(parts[1]) == 0 {
			return parts[0]
		}

		return line
	case 3:
		// The string started with NUL and and ended with NUL
		return parts[1]
	default:

		return line
	}
}

// Used to sort the lines of data in a series by their key, i.e the line number.
func SortLinesOfData(d map[int]*LineOfData) map[int]*LineOfData {
	keys := make([]int, 0, len(d))
	for k := range d {
		keys = append(keys, k)
	}
	sort.Ints(keys)

	m := make(map[int]*LineOfData)
	for _, k := range keys {
		m[k] = d[k]
	}
	return m
}

