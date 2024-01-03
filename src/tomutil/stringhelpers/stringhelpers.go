// Package stringhelpers is a collection of functions to manipulate strings.
package stringhelpers

import (
	"math"
	"strconv"
	"strings"
	"time"
	"unicode/utf8"
)

// TrimLastChar will remove the last character (rune) from a string.
func TrimLastChar(s string) string {
	r, size := utf8.DecodeLastRuneInString(s)
	if r == utf8.RuneError && (size == 0 || size == 1) {
		size = 0
	}
	return s[:len(s)-size]
}

// RemoveEmptyStrings will take an array and remove any strings that are empty.
func RemoveEmptyStrings(s []string) []string {
	var r []string
	for _, str := range s {
		if str != "" {
			r = append(r, str)
		}
	}
	return r
}

// ToTime will take two strings are return a time.Time value.
func ToTime(d string, t string) time.Time {
	myDate, err := time.Parse("01-02-2006 15:04:05", d+" "+t)
	if err != nil {
		panic(err)
	}

	return myDate
}

// IsNumericOnly will return true if all the characters in a string are numeric.
func IsNumericOnly(str string) bool {

	if len(str) == 0 {
		return false
	}

	for _, s := range str {
		if s < '0' || s > '9' {
			return false
		}
	}
	return true
}

// PadLeft will take an integer, and pad it with zeros on the left so that the string is the specified size.
// For example PadLeft(12, 4) will return "0012"
func PadLeft(v int64, length int) string {
	abs := math.Abs(float64(v))
	var padding int
	if v != 0 {
		miniumum := math.Pow10(length - 1)

		if miniumum-abs > 0 {
			l := math.Log10(abs)
			if l == float64(int64(l)) {
				l++
			}
			padding = length - int(math.Ceil(l))
		}
	} else {
		padding = length - 1
	}
	builder := strings.Builder{}
	if v < 0 {
		length = length + 1
	}
	builder.Grow(length * 4)
	if v < 0 {
		builder.WriteRune('-')
	}
	for i := 0; i < padding; i++ {
		builder.WriteRune('0')
	}
	builder.WriteString(strconv.FormatInt(int64(abs), 10))
	return builder.String()
}
