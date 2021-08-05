package util

import (
	"fmt"
	jww "github.com/spf13/jwalterweatherman"
	"math"
	"os"
	"strings"
	"unicode/utf8"
)

const UnicodeNUL = "\u0000"
const HexNUL = "\x00"

func GetHomeDir() string {
	homedir, err := os.UserHomeDir()
	if err != nil {
		jww.FATAL.Fatal(err)
	}
	return homedir
}

func FixupLabradarLine(line string) string {
	parts := strings.Split(strings.TrimSpace(line), UnicodeNUL)

	switch lengthOfParts := len(parts); lengthOfParts {
	case 2:
		// The string either started with a NUL or ended with a NUL
		if len(parts[0]) == 0 {
			return parts[1]
		}
		if len(parts[1]) == 0 {
			return parts[0]
		}
		jww.WARN.Println("Don't know how to handle the line " + line)
		return line
	case 3:
		// The string started with NUL and and ended with NUL
		return parts[1]
	default:
		jww.WARN.Println("Don't know how to handle the line " + line)
		return line
	}

}

func TrimLastChar(s string) string {
	r, size := utf8.DecodeLastRuneInString(s)
	if r == utf8.RuneError && (size == 0 || size == 1) {
		size = 0
	}
	return s[:len(s)-size]
}

func FormatLabradarSeriesNumber(seriesNumber int) string {
	return fmt.Sprintf("SR%04d", seriesNumber)
}

func GetPathToLabradarSeries(seriesNumber int) string {
	type fileParts struct {
		InputNameParts []string
		PathSep        string
		HomeDir        string
		LbrToken       string
	}

	var parts = &fileParts{
		[]string{"work", "labradar", "LBR"},
		string(os.PathSeparator),
		GetHomeDir(),
		FormatLabradarSeriesNumber(seriesNumber),
	}
	var pathToSeries = parts.HomeDir + parts.PathSep
	for _, part := range parts.InputNameParts {
		pathToSeries += part
		pathToSeries += parts.PathSep
	}
	pathToSeries += parts.LbrToken + parts.PathSep + parts.LbrToken + " Report.csv"
	return pathToSeries

}

func GetMaxAndMin(array []int) (int, int) {
	max := array[0]
	min := array[0]
	for _, value := range array {
		if max < value {
			max = value
		}
		if min > value {
			min = value
		}
	}
	return min, max
}

func CalculateAverage(array []int) float64 {
	count := len(array)
	sum := 0
	for _, velocity := range array {
		sum += velocity
	}

	value := float64(sum) / float64(count)
	valRounded :=  math.Round(value*10)/10
	return valRounded
}

func CalculateStandardDeviation(array []int) float64 {
	count := len(array)
	mean := CalculateAverage(array)
	var sd float64
	sd = float64(0)
	for j := 0; j < count; j++ {
		sd += math.Pow(float64(array[j])-mean, 2)
	}

	value := math.Sqrt(sd / float64(count))
	valRounded := math.Round(value*10)/10
	return valRounded
}
