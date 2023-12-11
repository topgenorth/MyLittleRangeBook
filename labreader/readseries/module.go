package readseries

import (
	"context"
	"labreader/internal/monolith"
)

type Module struct{}

func (Module) Startup(ctx context.Context, mono monolith.Monolith) error {

	return nil

}
