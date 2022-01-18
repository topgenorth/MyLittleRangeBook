package labradar

import (
	"opgenorth.net/mylittlerangebook/pkg"
	"sort"
	"strconv"
	"strings"
	"time"
)

type OldLineOfData struct {
	LineNumber int    `json:"lineNumber"`
	Raw        string `json:"raw"`
	Value      string `json:"value"`
}

// NewLineOfData will create a new OldLineOfData, parsing out the value from it.
func NewLineOfData(lineNumber int, s string) *OldLineOfData {
	return &OldLineOfData{
		LineNumber: lineNumber,
		Raw:        s,
		Value:      parseLabradarLineOfText(s),
	}
}

func (l OldLineOfData) String() string {
	return l.Value
}

// StringValue will return the value of the line, as a string.
func (l *OldLineOfData) StringValue() string {
	parts := strings.Split(l.Value, ";")
	if len(parts) < 2 {
		return ""
	}
	return parts[1]
}

// IntValue will return the value of the line, as an integer
func (l *OldLineOfData) IntValue() int {
	parts := strings.Split(l.Value, ";")
	if len(parts) < 2 {
		return -1
	}
	i, _ := strconv.Atoi(parts[1])
	return i
}

//DateAndTime will return the date and time of the line.
func (l *OldLineOfData) DateAndTime() (string, string) {
	parts := strings.Split(l.Value, ";")
	x := len(parts)
	if x == 1 {
		return "", ""
	}

	t := parseDateAndTime(parts[x-3], parts[x-2])
	return t.Format("2006-Jan-02"), t.Format("15:04")
}

// parseDateAndTime will take two strings are return a time.Time value.
func parseDateAndTime(d string, t string) time.Time {
	myDate, err := time.Parse("01-02-2006 15:04:05", d+" "+t)
	if err != nil {
		panic(err)
	}

	return myDate
}

// parseLabradarLineOfText will tryn and parse out the value, as a string, from the text.
func parseLabradarLineOfText(line string) string {
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
		// The string started with NUL and ended with NUL
		return parts[1]
	default:
		return line
	}
}

// SortLinesOfData will sort the array according to their key (i.e. line Number)
func SortLinesOfData(d map[int]*OldLineOfData) map[int]*OldLineOfData {
	keys := make([]int, 0, len(d))
	for k := range d {
		keys = append(keys, k)
	}
	sort.Ints(keys)

	m := make(map[int]*OldLineOfData)
	for _, k := range keys {
		m[k] = d[k]
	}
	return m
}
