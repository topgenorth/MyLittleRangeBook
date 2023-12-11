package config

type AppConfig struct {
	Environment string
	LogLevel    string
	ConfigFile  string
}

func InitConfig() (cfg AppConfig, err error) {
	//if err = dotenv.Load(dotenv.EnvironmentFiles(os.Getenv("ENVIRONMENT"))); err != nil {
	//	return
	//}

	//err = envconfig.Process("", &cfg)

	return
}
