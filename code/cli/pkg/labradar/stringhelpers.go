package labradar

import (
	"opgenorth.net/mylittlerangebook/pkg/labradar/series"
	"strconv"
	"strings"
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

func parsePowderString(powder string) *series.PowderCharge {
	parts := RemoveEmptyStrings(strings.Split(powder, " "))
	if len(parts) < 1 {
		return &series.PowderCharge{Name: "Unknown", Amount: 0.0}
	}

	p := &series.PowderCharge{
		Name:   parseNameOfProjectileFromString(strings.Join(parts[1:], " ")),
		Amount: parseAmountFromPowderString(parts[0]),
	}
	return p
}

func parseProjectileString(projectile string) *series.Projectile {
	parts := RemoveEmptyStrings(strings.Split(projectile, " "))

	if len(parts) < 1 {
		return &series.Projectile{Name: "Unknown", Weight: 0, BC: nil}
	}

	p := &series.Projectile{
		Name:   parseNameOfProjectileFromString(strings.Join(parts[1:], " ")),
		Weight: parseWeightFromProjectileString(parts[0]),
		BC:     nil, // [TO20220106] We don't worry about BC right now.
	}

	return p
}

func parseNameOfProjectileFromString(name string) string {

	replacer := strings.NewReplacer(
		"grains", "",
		"grain", "",
		"gr.", "",
		"gr", "",
	)
	return strings.TrimSpace(replacer.Replace(name))
}

func parseAmountFromPowderString(amount string) float32 {

	replacer := strings.NewReplacer(
		"grains", "",
		"grain", "",
		"gr.", "",
		"gr", "",
	)

	str := strings.TrimSpace(replacer.Replace(amount))

	w, err := strconv.ParseFloat(str, 32)
	if err != nil {
		return 0.0
	}

	return float32(w)
}

func parseWeightFromProjectileString(weight string) int {

	replacer := strings.NewReplacer(
		"grains", "",
		"grain", "",
		"gr.", "",
		"gr", "",
	)

	str := strings.TrimSpace(replacer.Replace(weight))

	w, err := strconv.ParseFloat(str, 32)
	if err != nil {
		return 0
	}

	return int(w)
}

// ToTime will take two strings are return a time.Time value.
func ToTime(d string, t string) time.Time {
	myDate, err := time.Parse("01-02-2006 15:04:05", d+" "+t)
	if err != nil {
		panic(err)
	}

	return myDate
}