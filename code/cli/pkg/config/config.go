package config

import (
	"fmt"
	"github.com/pkg/errors"
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

type Config struct {
	*context.AppContext

	// ConfigFilePath is the path the loaded configuration file.
	ConfigFilePath string

	// store the resolved home directory for MLRB
	mlrbHome string

	// Store the path to the executable.
	mlrbPath string
}

func New() *Config {
	ctx := context.New()
	c := &Config{
		AppContext: ctx,
	}

	p, err := c.GetHomeDir()
	if err != nil {
		c.mlrbHome = ""
	} else {
		c.mlrbHome = p
	}

	p, err = c.GetMlrbPath()
	if err != nil {
		c.mlrbPath = ""
	} else {
		c.mlrbPath = p
	}

	return c
}

func (c *Config) GetHomeDir() (string, error) {
	if c.mlrbHome != "" {
		return c.mlrbHome, nil
	}

	home := c.GetEnv(CONFIG_ENVIRONMENT_HOME_DIRECTORY)
	if home == "" {
		userHome, err := os.UserHomeDir()
		if err != nil {
			return "", errors.Wrap(err, "could not get user home directory")
		}
		home = filepath.Join(userHome, ".mlrb")
	}

	c.SetHomeDir(home)
	return c.mlrbHome, nil
}

// SetHomeDir is a test function that allows tests to use an alternate
// mlrb home directory.
func (c *Config) SetHomeDir(home string) {
	c.mlrbHome = home

	// Set this as an environment variable so that when we spawn new processes such as a mixin or plugin, that they can
	// find LABRADAR_HOME too
	c.SetEnv(CONFIG_ENVIRONMENT_HOME, home)
}

func (c *Config) GetMlrbPath() (string, error) {
	if c.mlrbPath != "" {
		return c.mlrbPath, nil
	}

	path, err := getExecutable()
	if err != nil {
		return "", errors.Wrap(err, "could not get path to the executing mlrb binary")
	}
	// We try to resolve back to the original location
	hardPath, err := evalSymlinks(path)
	if err != nil { // if we have trouble resolving symlinks, skip trying to help people who used symlinks
		fmt.Fprintln(c.Err, errors.Wrapf(err, "WARNING could not resolve %s for symbolic links\n", path))
	} else if hardPath != path {
		if c.Debug {
			fmt.Fprintf(c.Err, "Resolved mlrb binary from %s to %s\n", path, hardPath)
		}
		path = hardPath
	}

	c.mlrbPath = path
	return path, nil
}

// SetMlrbPath is a test function that allows tests to use an alternate
// Mlrb binary location.
func (c *Config) SetMlrbPath(path string) {
	c.mlrbPath = path
}
