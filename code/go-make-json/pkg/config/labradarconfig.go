package config

type LabradarConfig struct {
	InputDir  string `mapstructure:"inputdir"`
	OutputDir string `mapstructure:"outputDir"`
}
