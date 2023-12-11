package application

import (
	"context"
	"github.com/rs/zerolog"
	"labreader/internal/config"
	"labreader/internal/logger"
)

type (
	SeriesToLoad struct {
		SeriesNumber int
		Directory    string
	}

	ShotInSeries struct {
		Number         int
		MuzzleVelocity int
		TimeOfShot     string
	}

	App interface {
		LoadSeries(ctx context.Context, load SeriesToLoad) error
	}
	Application struct {
		App
		cfg    config.AppConfig
		logger *zerolog.Logger
	}
)

func (a Application) Logger() *zerolog.Logger {
	return a.logger
}

func (a Application) Config() config.AppConfig {
	return a.cfg
}

func NewReadStatsApplication() App {
	var a = &Application{
		App:    nil,
		cfg:    config.AppConfig{},
		logger: logger.DefaultLogger(),
	}

	return a
}
