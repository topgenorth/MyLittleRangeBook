package config

import (
	"github.com/sirupsen/logrus"
	"opgenorth.net/mylittlerangebook/pkg/context"
	"os"
	"path/filepath"
	"runtime"
	"strings"
)

const (
	// MlrbConfigFileName is the file name of the  configuration file, but without any extension
	MlrbConfigFileName = "mlrb"
	MlrbConfigFileType = "toml"

	// MlrbEnvironmentVariablePrefix is the prefix for all environment variables.
	MlrbEnvironmentVariablePrefix = "MLRB"
)

// Config holds the current context.AppContext along with some other configuration specific stuff.
type Config struct {
	*context.AppContext

	// ConfigFilePath is the path the loaded configuration file.
	ConfigFilePath string
}

// New will create a new Config structure.
func New() *Config {
	ctx := context.New()
	c := &Config{
		AppContext:     ctx,
		ConfigFilePath: defaultConfigFile(),
	}

	return c
}

func absPathify(inPath string) string {

	// [TO20220121] Shamelessly stolen from Viper
	logrus.Tracef("Trying to resolve absolute path to %s.", inPath)

	if inPath == "$HOME" || strings.HasPrefix(inPath, "$HOME"+string(os.PathSeparator)) {
		inPath = userHomeDir() + inPath[5:]
	}

	if strings.HasPrefix(inPath, "$") {
		end := strings.Index(inPath, string(os.PathSeparator))

		var value, suffix string
		if end == -1 {
			value = os.Getenv(inPath[1:])
		} else {
			value = os.Getenv(inPath[1:end])
			suffix = inPath[end:]
		}

		inPath = value + suffix
	}

	if filepath.IsAbs(inPath) {
		return filepath.Clean(inPath)
	}

	p, err := filepath.Abs(inPath)
	if err == nil {
		return filepath.Clean(p)
	}

	logrus.Errorf("Couldn't discover absolute path %v", err)
	return ""
}

func userHomeDir() string {
	if runtime.GOOS == "windows" {
		home := os.Getenv("HOMEDRIVE") + os.Getenv("HOMEPATH")
		if home == "" {
			home = os.Getenv("USERPROFILE")
		}
		return home
	}
	return os.Getenv("HOME")
}

func defaultConfigFile() string {

	d := absPathify(".")
	filename := MlrbConfigFileName + MlrbConfigFileType

	return filepath.Join(d, filename)
}
