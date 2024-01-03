package catalog

import (
	"fmt"
	constants "opgenorth.net/tomutil"
	"os"
	"path/filepath"
	"strconv"
	"strings"
)

// commandLineValues holds the config/command line values for the action.
type commandLineValues struct {
	Rifle         string  `mapstructure:"rifle"`
	Cartridge     string  `mapstructure:"cartridge"`
	Powder        string  `mapstructure:"powder"`
	PowderCharge  float64 `mapstructure:"powder-charge"`
	Bullet        string  `mapstructure:"bullet"`
	BulletWeight  int     `mapstructure:"bullet-weight"`
	COAL          float64 `mapstructure:"coal"`
	CBTO          float64 `mapstructure:"cbto"`
	Rename        bool    `mapstructure:"rename"`
	Dryrun        bool    `mapstructure:"dryrun"`
	CatalogFolder string  `mapstructure:"LBR_CATALOG_FOLDER"`
}

func (b commandLineValues) getBullet() bulletData {
	return bulletData{
		Name:   b.Bullet,
		Weight: b.BulletWeight,
	}
}

func (b commandLineValues) getPowder() powderData {
	return powderData{
		Name:   b.Powder,
		Weight: b.PowderCharge,
	}
}

// String will convert the meta-data to a file system friendly string.
func (b commandLineValues) String() string {
	var s strings.Builder

	s.WriteString(b.Rifle)
	s.WriteString("-")

	// TODO [TO20231208] Need to clean the cartridge (spaces, illegal characters, etc)
	s.WriteString(strings.ReplaceAll(b.Cartridge, ".", ""))

	// TODO [TO20231208] Need to clean the bullet (spaces, illegal characters, etc)
	formattedBullet := b.getBullet().String()
	if formattedBullet != "" {
		s.WriteString("-")
		s.WriteString(strings.ReplaceAll(formattedBullet, ".", ""))
	}

	// TODO [TO20231208] Need to clean the Powder (spaces, illegal characters, etc).
	formattedPowder := b.getPowder().String()
	if formattedPowder != "" {
		s.WriteString("-")
		s.WriteString(formattedPowder)
	}

	if b.CBTO > 0 {
		s.WriteString("-CBTO-")
		s.WriteString(floatToStr(b.CBTO, 3))
	} else if b.COAL > 0 {
		s.WriteString("-COAL-")
		s.WriteString(floatToStr(b.COAL, 3))
	}

	return s.String()
}

type bulletData struct {
	Name   string
	Weight int
}

func (b bulletData) String() string {
	if b.Name == constants.UnknownStr {
		return ""
	}
	if strings.TrimSpace(b.Name) == "" {
		return ""
	}

	var s strings.Builder
	s.WriteString(strings.ReplaceAll(b.Name, " ", "_"))

	if b.Weight > 0 {
		s.WriteString("-")
		v := strconv.Itoa(b.Weight)
		s.WriteString(v)
	}

	return s.String()
}

/**--- */
type powderData struct {
	Name   string
	Weight float64
}

func (pd powderData) String() string {
	if pd.Name == constants.UnknownStr {
		return ""
	}

	if strings.TrimSpace(pd.Name) == "" {
		return ""
	}

	var s strings.Builder
	s.WriteString(strings.ReplaceAll(pd.Name, " ", "_"))

	if pd.Weight > 0 {
		s.WriteString("-")
		s.WriteString(floatToStr(pd.Weight, 1))
	}

	return s.String()
}

// floatToStr will take a float and format it to a string in the format 999_0.  Suitable for weight (grains).
func floatToStr(val float64, precision int) string {
	if val > 0 {
		format := "%." + strconv.Itoa(precision) + "f"
		f := strings.ReplaceAll(fmt.Sprintf(format, val), ".", "_")
		return f
	}
	return ""
}

// isValidPath is a wrapper around os.Stat.
func isValidPath(path string) bool {
	_, err := os.Stat(filepath.FromSlash(path))
	return err != nil
}
