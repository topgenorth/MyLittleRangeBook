package cmd

import (
	"fmt"
	"github.com/spf13/cobra"
	"github.com/spf13/pflag"
	"github.com/spf13/viper"
	"opgenorth.net/labradar/pkg/config"
	"opgenorth.net/labradar/pkg/labradar"
	"strings"
)


func readLabradarCsvAndConvertToJson(seriesNumber int, cfg *config.Config) error {
	ls := labradar.NewSeries(seriesNumber, cfg)
	err := labradar.LoadLabradarSeriesFromCsv(ls, cfg)
	if err != nil {
		return err
	}

	err2 := labradar.SaveLabradarSeriesToJson(ls, cfg)
	if err2 != nil {
		return err2
	}

	return nil
}
