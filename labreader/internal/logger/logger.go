package logger

import (
	"labreader/internal/config"
	"os"

	"github.com/rs/zerolog"
	"github.com/rs/zerolog/pkgerrors"
)

const defaultTimeFormat = "03:04:05.000PM"

type loggerOption func(*config.LogConfig)

// New will create a new zerolog.Logger that is configured for a production environment, logging only INFO or higher.
func New(options ...loggerOption) zerolog.Logger {
	cfg := &config.LogConfig{Env: config.PRODUCTION, LogLevel: config.LogInfo}
	for _, o := range options {
		o(cfg)
	}

	l := createLoggerInstance(*cfg)
	return l
}

func DevelopmentEnvironment() func(*config.LogConfig) {
	return func(logConfig *config.LogConfig) {
		logConfig.Env = config.DEVELOPMENT
	}
}

func LogLevelDebug() func(*config.LogConfig) {
	return func(logConfig *config.LogConfig) {
		logConfig.LogLevel = config.LogDebug
	}
}

func createLoggerInstance(cfg config.LogConfig) zerolog.Logger {
	zerolog.TimeFieldFormat = zerolog.TimeFormatUnixMs
	zerolog.ErrorStackMarshaler = pkgerrors.MarshalStack

	switch cfg.Env {
	case config.PRODUCTION:
		return zerolog.New(os.Stdout).
			Level(logLevelToZero(cfg.LogLevel)).
			With().
			Timestamp().
			Logger()
	default:
		return zerolog.New(zerolog.NewConsoleWriter(func(w *zerolog.ConsoleWriter) {

			w.TimeFormat = defaultTimeFormat
		})).
			Level(logLevelToZero(cfg.LogLevel)).
			With().
			Timestamp().
			Logger()
	}
}

// logLevelToZero will take our own log levels and map those to a ZeroLog value.
func logLevelToZero(level config.LogLevel) zerolog.Level {
	switch level {
	case config.LogPanic:
		return zerolog.PanicLevel
	case config.LogError:
		return zerolog.ErrorLevel
	case config.LogWarn:
		return zerolog.WarnLevel
	case config.LogInfo:
		return zerolog.InfoLevel
	case config.LogDebug:
		return zerolog.DebugLevel
	case config.LogTrace:
		return zerolog.TraceLevel
	default:
		return zerolog.InfoLevel
	}
}
