package commands

import "context"

type (
	CreateSeries struct {
		ID string
	}

	CreateSeriesHandler struct {
	}
)

func (h *CreateSeriesHandler) CreateSeries(ctx context.Context, cmd CreateSeries) error {
	return nil
}
