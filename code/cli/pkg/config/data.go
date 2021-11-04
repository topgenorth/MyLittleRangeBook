package config

// Data is the data stored in MLRB_HOME/config.toml
// Use the accessor functions to ensure default values are handled properly.
type Data struct {
	// Only define fields here that you need to access from code
	// Values are dynamically applied to flags and don't need to be defined

	// Labradar is the section where to find the CSV files and save the JSON files.
	Labradar LabradarConfig `mapstructure:"labradar"`

	Aws AwsConfig `mapstructure:"aws"`
}

type AwsConfig struct {
	Region          string
	AccessKeyId     string
	SecretAccessKey string
}

type LabradarConfig struct {
	InputDir  string `mapstructure:"inputdir"`
	OutputDir string `mapstructure:"outputDir"`
}


