package config

type Environment string
type LogLevel string

type LogConfig struct {
	Env      Environment
	LogLevel LogLevel
}

const (
	DEVELOPMENT Environment = "DEVELOPMENT"
	PRODUCTION  Environment = "PRODUCTION"
	LogTrace    LogLevel    = "TRACE"
	LogDebug    LogLevel    = "DEBUG"
	LogInfo     LogLevel    = "INFO"
	LogWarn     LogLevel    = "WARN"
	LogError    LogLevel    = "ERROR"
	LogPanic    LogLevel    = "PANIC"
)
