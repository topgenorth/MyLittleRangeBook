package labradar

import (
	"opgenorth.net/mylittlerangebook/pkg/config"
	"os"
)

func DeleteFile(s string, c *config.Config) bool {
	exists, err := c.FileSystem.Exists(s)
	if err != nil {
		return false
	}
	if exists {
		err := os.Remove(s)
		// TODO [TO20220105] Log a warning.
		if err != nil {
			return false
		}
	}
	return true
}
