package monolith

import (
	"context"
	"github.com/rs/zerolog"
	"labreader/internal/config"
)

type Monolith interface {
	Config() config.AppConfig
	Logger() zerolog.Logger
}

type Module interface {
	Startup(context.Context, Monolith) error
}
