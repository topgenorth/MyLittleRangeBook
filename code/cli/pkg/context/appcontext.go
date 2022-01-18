package context

import (
	"fmt"
	"github.com/carolynvs/aferox"
	"github.com/pkg/errors"
	"github.com/spf13/afero"
	"io"
	"os"
	"path/filepath"
	"strings"
	"time"
)

const (
	DefaultTimeZone = "America/Edmonton"
)

type AppContext struct {
	Debug      bool
	verbose    bool
	environ    map[string]string
	FileSystem aferox.Aferox
	In         io.Reader
	Out        io.Writer
	Err        io.Writer
	Timezone   string
}

func New() *AppContext {

	pwd, _ := os.Getwd()

	return &AppContext{
		Debug:      true,
		environ:    getEnviron(),
		In:         os.Stdin,
		Out:        os.Stdout,
		Err:        os.Stderr,
		Timezone:   inferDefaultTimeZone(),
		FileSystem: aferox.NewAferox(pwd, afero.NewOsFs()),
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

// Getwd returns a rooted path name corresponding to the current directory.
func (c *AppContext) Getwd() string {
	return c.FileSystem.Getwd()
}

// Chdir changes the current working directory to the named directory.
func (c *AppContext) Chdir(dir string) {
	c.FileSystem.Chdir(dir)
}

func (c *AppContext) TimeLocation() *time.Location {
	tz, err := time.LoadLocation(c.Timezone)
	if err == nil {
		return tz
	} else {
		c.Timezone = DefaultTimeZone
		tz, _ := time.LoadLocation(DefaultTimeZone)
		return tz
	}
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

func (c *AppContext) CopyFile(src, dest string) error {
	info, err := c.FileSystem.Stat(src)
	if err != nil {
		return errors.WithStack(err)
	}

	data, err := c.FileSystem.ReadFile(src)
	if err != nil {
		return errors.WithStack(err)
	}

	err = c.FileSystem.WriteFile(dest, data, info.Mode())
	return errors.WithStack(err)
}

func (c *AppContext) CopyDirectory(srcDir, destDir string, includeBaseDir bool) error {
	var stripPrefix string
	if includeBaseDir {
		stripPrefix = filepath.Dir(srcDir)
	} else {
		stripPrefix = srcDir
	}

	return c.FileSystem.Walk(srcDir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return errors.WithStack(err)
		}

		// Translate the path from the src to the final destination
		dest := filepath.Join(destDir, strings.TrimPrefix(path, stripPrefix))
		if dest == "" {
			return nil
		}

		if info.IsDir() {
			return errors.WithStack(c.FileSystem.MkdirAll(dest, info.Mode()))
		}

		return c.CopyFile(path, dest)
	})
}

func inferDefaultTimeZone() string {
	zone, _ := time.Now().Zone() // try to get my time zone...
	loc, _ := time.LoadLocation(zone)
	return loc.String()
}
