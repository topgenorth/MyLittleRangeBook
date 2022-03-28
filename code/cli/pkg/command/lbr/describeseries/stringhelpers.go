package describeseries

import (
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"opgenorth.net/mylittlerangebook/pkg/util"
	"strconv"
	"strings"
)

func parsePowderString(powder string) *labradar.PowderCharge {
	parts := util.RemoveEmptyStrings(strings.Split(powder, " "))
	if len(parts) < 1 {
		return &labradar.PowderCharge{Name: "Unknown", Amount: 0.0}
	}

	p := &labradar.PowderCharge{
		Name:   parseNameOfProjectileFromString(strings.Join(parts[1:], " ")),
		Amount: parseAmountFromPowderString(parts[0]),
	}
	return p
}

func parseProjectileString(projectile string) *labradar.Projectile {
	parts := util.RemoveEmptyStrings(strings.Split(projectile, " "))

	if len(parts) < 1 {
		return &labradar.Projectile{Name: "Unknown", Weight: 0, BC: nil}
	}

	p := &labradar.Projectile{
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
