package config

import (
	"opgenorth.net/mylittlerangebook/fs"
	"opgenorth.net/mylittlerangebook/pkg/context"
	"path/filepath"
)

const (
	// MlrbConfigFileName is the file name of the  configuration file, but without any extension
	MlrbConfigFileName = "mlrb"
	// MlrbConfigFileType is the type of config file used, TOML in this case.
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

func defaultConfigFile() string {

	d := fs.AbsPathify(".")
	filename := MlrbConfigFileName + MlrbConfigFileType

	return filepath.Join(d, filename)
}
