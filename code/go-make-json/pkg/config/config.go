package config

import (
	"fmt"
	"github.com/pkg/errors"
	"github.com/spf13/afero"
	"opgenorth.net/labradar/pkg/context"
	"os"
	"path/filepath"
)

const (
	// Filename is the file name of the  configuration file.
	Filename = "mlrb"

	// EnvPREFIX is the prefix for all environment variables.
	EnvPREFIX = "MLRB"

	// EnvHOME is the name of the environment variable containing the mlrb home directory path.
	EnvHOME = "MLRB_HOME"

	// EnvDEBUG is a custom  parameter that signals that --debug flag has been passed through from the client to the runtime.
	EnvDEBUG = "MLRB_DEBUG"

	// EnvTIMEZONE is the name of the environment variable that holds the IANA Timezone of the Labradar.
	EnvTIMEZONE = "MLRB_TIMEZONE"
)

// These are functions that afero doesn't support, so this lets us stub them out for tests to set the
// location of the current executable mlrb binary and resolve MLRB_HOME.
var getExecutable = os.Executable
var evalSymlinks = filepath.EvalSymlinks

type DataStoreLoaderFunc func(*Config) error

var _ DataStoreLoaderFunc = NoopDataLoader

// NoopDataLoader doesn't do anything.
func NoopDataLoader(config *Config) error {
	return nil
}

type Config struct {
	*context.Context
	Data       *Data
	DataLoader DataStoreLoaderFunc

	// ConfigFilePath is the path the loaded configuration file.
	ConfigFilePath string

	// store the resolved home directory for MLRB
	mlrbHome string

	// Store the path to the executable.
	mlrbPath string

	Filesystem afero.Fs
}

func New() *Config {
	c := context.New()
	return &Config{
		Context:    c,
		DataLoader: NoopDataLoader,
	}
}

// LoadData from the datastore in MLRB_HOME.
// This defaults to doing nothing unless DataLoader has been set.
func (c *Config) LoadData() error {
	c.Data = nil

	if c.DataLoader == nil {
		c.DataLoader = NoopDataLoader
	}

	return c.DataLoader(c)
}

func (c *Config) GetHomeDir() (string, error) {
	if c.mlrbHome != "" {
		return c.mlrbHome, nil
	}

	home := c.Getenv(EnvHOME)
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

	// Set this as an environment variable so that when we spawn new processes
	// such as a mixin or plugin, that they can find LABRADAR_HOME too
	c.Setenv(EnvHOME, home)
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
