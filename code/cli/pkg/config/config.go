package config

import (
	"opgenorth.net/mylittlerangebook/pkg/context"
	"os"
	"path/filepath"
)

const (
	// CONFIG_FILENAME is the file name of the  configuration file, but without any extension
	CONFIG_FILENAME = "mlrb"

	// CONFIG_ENVIRONMENT_VARIABLE_PREFIX is the prefix for all environment variables.
	CONFIG_ENVIRONMENT_VARIABLE_PREFIX = "MLRB"

	// CONFIG_ENVIRONMENT_HOME_DIRECTORY is the name of the environment variable containing the mlrb home directory path.
	CONFIG_ENVIRONMENT_HOME_DIRECTORY = "MLRB_HOME"

	CONFIG_ENVIRONMENT_HOME = "MLRB_HOME"
)

// These are functions that afero doesn't support, so this lets us stub them out for tests to set the
// location of the current executable mlrb binary and resolve MLRB_HOME.
var getExecutable = os.Executable
var evalSymlinks = filepath.EvalSymlinks

// Config holds the current context.AppContext along with some other configuration specific stuff.
type Config struct {
	*context.AppContext

	// ConfigFilePath is the path the loaded configuration file.
	ConfigFilePath string
}

func New() *Config {
	ctx := context.New()
	c := &Config{
		AppContext: ctx,
	}

	return c
}
