// Package logger is a wrapper around zerolog.
package logger

import (
	"github.com/rs/zerolog"
	"github.com/rs/zerolog/pkgerrors"
	"opgenorth.net/tomutil"
	"os"
)

const defaultTimeFormat = "03:04:05.000PM"

type LogLevel string

type loggerOption func(*LogConfig)

const (
	LogTrace LogLevel = "TRACE"
	LogDebug LogLevel = "DEBUG"
	LogInfo  LogLevel = "INFO"
	LogWarn  LogLevel = "WARN"
	LogError LogLevel = "ERROR"
	LogPanic LogLevel = "PANIC"
)

type LogConfig struct {
	env      tomutil.RuntimeEnvironment
	logLevel LogLevel
}

// New will create a new zerolog.Logger that is configured for a production environment, logging only INFO or higher.
func New(options ...loggerOption) zerolog.Logger {
	cfg := &LogConfig{env: tomutil.ProductionEnvironment, logLevel: LogInfo}
	for _, o := range options {
		o(cfg)
	}

	l := createLoggerInstance(*cfg)
	return l
}

func DevelopmentEnvironment() func(*LogConfig) {
	return func(logConfig *LogConfig) {
		logConfig.env = tomutil.ProductionEnvironment
	}
}

func LogLevelDebug() func(*LogConfig) {
	return func(logConfig *LogConfig) {
		logConfig.logLevel = LogDebug
	}
}

func createLoggerInstance(cfg LogConfig) zerolog.Logger {
	zerolog.TimeFieldFormat = zerolog.TimeFormatUnixMs
	zerolog.ErrorStackMarshaler = pkgerrors.MarshalStack

	switch cfg.env {
	case tomutil.ProductionEnvironment:
		return zerolog.New(os.Stdout).
			Level(logLevelToZero(cfg.logLevel)).
			With().
			Timestamp().
			Logger()
	default:
		return zerolog.New(zerolog.NewConsoleWriter(func(w *zerolog.ConsoleWriter) {

			w.TimeFormat = defaultTimeFormat
		})).
			Level(logLevelToZero(cfg.logLevel)).
			With().
			Timestamp().
			Logger()
	}
}

// logLevelToZero will take our own log levels and map those to a ZeroLog value.
func logLevelToZero(level LogLevel) zerolog.Level {
	switch level {
	case LogPanic:
		return zerolog.PanicLevel
	case LogError:
		return zerolog.ErrorLevel
	case LogWarn:
		return zerolog.WarnLevel
	case LogInfo:
		return zerolog.InfoLevel
	case LogDebug:
		return zerolog.DebugLevel
	case LogTrace:
		return zerolog.TraceLevel
	default:
		return zerolog.InfoLevel
	}
}
