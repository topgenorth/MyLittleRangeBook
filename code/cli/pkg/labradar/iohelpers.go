// Package fs encapsulates all of the code for manipulating the files created by a Labradar.
package labradar

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg"
	"strings"
)

func CloseFile(f afero.File) {
	err := f.Close()
	if err != nil {
		logrus.Error(err)
	}
}

// SanitizeLine will take a line of text (from a Labradar CSV file) and try to clean up some of the odd things
// that the Labradar writes out.
func SanitizeLine(line string) string {
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
