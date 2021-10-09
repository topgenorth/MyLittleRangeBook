package context

import (
	"fmt"
	"github.com/spf13/afero"
	"io"
	"os"
	"strings"
	"time"
)

const (
	DefaultTimeZone = "America/Edmonton"
)

type Context struct {
	Debug      bool
	verbose    bool
	environ    map[string]string
	In         io.Reader
	Out        io.Writer
	Err        io.Writer
	Timezone   string
	Filesystem afero.Afero
}

func New() *Context {
	return &Context{
		environ:  getEnviron(),
		In:       os.Stdin,
		Out:      os.Stdout,
		Err:      os.Stderr,
		Timezone: DefaultTimeZone,
		Filesystem: afero.Afero{
			Fs: afero.NewOsFs(),
		},
	}
}

// Environ returns a copy of strings representing the environment,
// in the form "key=value".
func (c *Context) Environ() []string {
	e := make([]string, 0, len(c.environ))
	for k, v := range c.environ {
		e = append(e, fmt.Sprintf("%s=%s", k, v))
	}
	return e
}

// EnvironMap returns a map of the current environment variables.
func (c *Context) EnvironMap() map[string]string {
	env := make(map[string]string, len(c.environ))
	for k, v := range c.environ {
		env[k] = v
	}
	return env
}

func (c *Context) SetVerbose(value bool) {
	c.verbose = value
}

func (c *Context) IsVerbose() bool {
	return c.Debug || c.verbose
}

func (c *Context) TimeLocation() *time.Location {

	

	tz, err := time.LoadLocation(c.Timezone)
	if err == nil {
		return tz
	} else {
		c.Timezone = DefaultTimeZone
		tz, _ := time.LoadLocation(DefaultTimeZone)
		return tz
	}
}

// Getenv retrieves the value of the environment variable named by the key.
// It returns the value, which will be empty if the variable is not present.
// To distinguish between an empty value and an unset value, use LookupEnv.
func (c *Context) Getenv(key string) string {
	return c.environ[key]
}

// Setenv sets the value of the environment variable named by the key.
// It returns an error, if any.
func (c *Context) Setenv(key string, value string) {
	if c.environ == nil {
		c.environ = make(map[string]string, 1)
	}

	c.environ[key] = value
}
// LookupEnv retrieves the value of the environment variable named
// by the key. If the variable is present in the environment the
// value (which may be empty) is returned and the boolean is true.
// Otherwise the returned value will be empty and the boolean will
// be false.
func (c *Context) LookupEnv(key string) (string, bool) {
	value, ok := c.environ[key]
	return value, ok
}

func getEnviron() map[string]string {
	environ := map[string]string{}
	for _, env := range os.Environ() {
		envParts := strings.SplitN(env, "=", 2)
		key := envParts[0]
		value := ""
		if len(envParts) > 1 {
			value = envParts[1]
		}
		environ[key] = value
	}
	return environ
}
