package config

import (
	"fmt"
	"github.com/pkg/errors"
	"github.com/spf13/afero"
	"github.com/spf13/cobra"
	"github.com/spf13/pflag"
	"github.com/spf13/viper"
	"opgenorth.net/labradar/pkg/context"
	"os"
	"path/filepath"
	"strings"
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
	DataLoader DataStoreLoaderFunc
	// ConfigFilePath is the path the loaded configuration file.
	ConfigFilePath string

	// store the resolved home directory for MLRB
	mlrbHome string

	// Store the path to the executable.
	mlrbPath string

	Filesystem     afero.Fs
	AwsConfig      *AwsConfig
	LabradarConfig *LabradarConfig
}

func New() *Config {
	c := context.New()
	return &Config{
		Context:    c,
		DataLoader: NoopDataLoader,

		Filesystem: afero.NewOsFs(),
		AwsConfig:  getAwsConfig(c.EnvironMap()),
	}
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

func (c *Config) ConfigCmd(cmd *cobra.Command) error {
	v := viper.New()

	// Set the base name of the config file, without the file extension.
	v.SetConfigName(Filename)

	// Set as many paths as you like where viper should look for the
	// config file. We are only looking in the current working directory.
	v.AddConfigPath(".")

	// Attempt to read the config file, gracefully ignoring errors
	// caused by a config file not being found. Return an error
	// if we cannot parse the config file.
	if err := v.ReadInConfig(); err != nil {
		// It's okay if there isn't a config file
		if _, ok := err.(viper.ConfigFileNotFoundError); !ok {
			return err
		}
	}

	var cfg MlrbConfig
	if err := v.Unmarshal(&cfg); err != nil {
		return err
	}

	// When we bind flags to environment variables expect that the
	// environment variables are prefixed, e.g. a flag like --number
	// binds to an environment variable STING_NUMBER. This helps
	// avoid conflicts.
	v.SetEnvPrefix(EnvPREFIX)

	// Bind to environment variables
	// Works great for simple config names, but needs help for names
	// like --favorite-color which we fix in the bindFlags function
	v.AutomaticEnv()

	// Bind the current command's flags to viper
	bindFlags(cmd, v)

	return nil
}

// bindFlags will bind each cobra flag to its associated viper configuration (config file and environment variable)
func bindFlags(cmd *cobra.Command, v *viper.Viper) {
	cmd.Flags().VisitAll(func(f *pflag.Flag) {
		// Environment variables can't have dashes in them, so bind them to their equivalent
		// keys with underscores, e.g. --favorite-color to LABRADAR_FAVORITE_COLOR
		if strings.Contains(f.Name, "-") {
			envVarSuffix := strings.ToUpper(strings.ReplaceAll(f.Name, "-", "_"))
			v.BindEnv(f.Name, fmt.Sprintf("%s_%s", EnvPREFIX, envVarSuffix))
		}

		// Apply the viper config value to the flag when the flag is not set and viper has a value
		if !f.Changed && v.IsSet(f.Name) {
			val := v.Get(f.Name)
			cmd.Flags().Set(f.Name, fmt.Sprintf("%v", val))
		}
	})
}

func getAwsConfig(env map[string]string) *AwsConfig {
	return &AwsConfig{
		Region:          env["AWS_REGION"],
		AccessKeyId:     env["AWS_ACCESS_KEY_ID"],
		SecretAccessKey: env["AWS_SECRET_ACCESS_KEY"],
	}
}
