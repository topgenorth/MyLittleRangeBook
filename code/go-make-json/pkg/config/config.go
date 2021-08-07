package config

import (
	"opgenorth.net/labradar/pkg/context"
	"time"
)

const (
	EnvPrefix             = "LABRADAR"
	DefaultConfigFileName = "make-json.yaml"
	DefaultOutputDir      = "/Users/tom/work/topgenorth.github.io/data/labradar/"
	DefaultInputDir       = "/Users/tom/work/labradar/LBR/"
	DefaultTimeZone       = "America/Edmonton"
)

type Config struct {
	Context   *context.Context
	InputDir  string
	OutputDir string
	TimeZone  *time.Location
}

func New() *Config {
	timezone, _ := time.LoadLocation(DefaultTimeZone)
	c := &Config{
		Context:   context.New(),
		InputDir:  DefaultInputDir,
		OutputDir: DefaultOutputDir,
		TimeZone:  timezone,
	}
	return c
}
