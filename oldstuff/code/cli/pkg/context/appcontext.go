package context

import (
	"fmt"
	"github.com/spf13/afero"
	"io"
	"opgenorth.net/mylittlerangebook/pkg"
	"os"
	"time"
)

// AppContext is the
type AppContext struct {
	// Filesystem is the afero wrapper around the filesystem.
	Filesystem *afero.Afero

	// Timezone is the string representation of the time.Location that the app is running in.
	Timezone string

	// Debug is a flag that is set when dev
	Debug   bool
	verbose bool

	// environ holds a list of all environment variables
	environ map[string]string

	In  io.Reader
	Out io.Writer
	Err io.Writer
}

func New() *AppContext {
	return &AppContext{
		Debug:      false,
		environ:    getEnviron(),
		In:         os.Stdin,
		Out:        os.Stdout,
		Err:        os.Stderr,
		Timezone:   InferTimeZone(),
		Filesystem: &afero.Afero{Fs: afero.NewOsFs()},
	}
}

func (c *AppContext) SetVerbose(value bool) {
	c.verbose = value
}

func (c *AppContext) IsVerbose() bool {
	return c.Debug || c.verbose
}

// Environ returns a copy of strings representing the environment,
// in the form "key=value".
func (c *AppContext) Environ() []string {
	e := make([]string, 0, len(c.environ))
	for k, v := range c.environ {
		e = append(e, fmt.Sprintf("%s=%s", k, v))
	}
	return e
}

// EnvironMap returns a map of the current environment variables.
func (c *AppContext) EnvironMap() map[string]string {
	env := make(map[string]string, len(c.environ))
	for k, v := range c.environ {
		env[k] = v
	}
	return env
}

// ExpandEnv replaces ${var} or $var in the string according to the values
// of the current environment variables. References to undefined
// variables are replaced by the empty string.
func (c *AppContext) ExpandEnv(s string) string {
	return os.Expand(s, func(key string) string { return c.GetEnv(key) })
}

// GetEnv retrieves the value of the environment variable named by the key.
// It returns the value, which will be empty if the variable is not present.
// To distinguish between an empty value and an unset value, use LookupEnv.
func (c *AppContext) GetEnv(key string) string {
	return c.environ[key]
}

// LookupEnv retrieves the value of the environment variable named
// by the key. If the variable is present in the environment the
// value (which may be empty) is returned and the boolean is true.
// Otherwise the returned value will be empty and the boolean will
// be false.
func (c *AppContext) LookupEnv(key string) (string, bool) {
	value, ok := c.environ[key]
	return value, ok
}

// SetEnv sets the value of the environment variable named by the key.
// It returns an error, if any.
func (c *AppContext) SetEnv(key string, value string) {
	if c.environ == nil {
		c.environ = make(map[string]string, 1)
	}

	c.environ[key] = value
}

// UnsetEnv unsets a single environment variable.
func (c *AppContext) UnsetEnv(key string) {
	delete(c.environ, key)
}

// ClearEnv deletes all environment variables.
func (c *AppContext) ClearEnv() {
	c.environ = make(map[string]string, 0)
}

// TimeLocation will return a pointer to a time.Location structure.
func (c *AppContext) TimeLocation() *time.Location {
	tz, err := time.LoadLocation(c.Timezone)
	if err == nil {
		return tz
	} else {
		c.Timezone = pkg.DefaultTimeZone
		tz, _ := time.LoadLocation(pkg.DefaultTimeZone)
		return tz
	}
}
