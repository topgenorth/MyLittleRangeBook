package util

import (
	"time"
	"unicode/utf8"
)

func TrimLastChar(s string) string {
	r, size := utf8.DecodeLastRuneInString(s)
	if r == utf8.RuneError && (size == 0 || size == 1) {
		size = 0
	}
	return s[:len(s)-size]
}

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
