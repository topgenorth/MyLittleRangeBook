package config

import (
	"opgenorth.net/labradar/pkg/context"
	"time"
)

const (
	EnvPrefix             = "LABRADAR"
	DefaultConfigFileName = "make-json"
	DefaultOutputDir      = "/Users/tom/work/topgenorth.github.io/data/labradar/"
	DefaultInputDir       = "/Users/tom/work/labradar/LBR/"
	DefaultTimeZone       = "America/Edmonton"
)

type Config struct {
	InputDir  string
	OutputDir string
	Context   *context.Context
	TimeZone  *time.Location
}

func New() *Config {
	timezone, _ := time.LoadLocation(DefaultTimeZone)
	c := &Config{
		Context:   context.New(),
		InputDir:  "",
		OutputDir: "",
		TimeZone:  timezone,
	}
	return c
}
