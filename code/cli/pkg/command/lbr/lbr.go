// Package lbr holds the code to build the cobra.Command structs for interacting with Labradar CSV files.
package lbr

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	describeseries "opgenorth.net/mylittlerangebook/pkg/command/lbr/describe"
	listseries "opgenorth.net/mylittlerangebook/pkg/command/lbr/list"
	readseries "opgenorth.net/mylittlerangebook/pkg/command/lbr/read"
	"opgenorth.net/mylittlerangebook/pkg/config"
)

const (
	LbrDirectoryFlagParam = "lbr_dir"
)

// NewLabradarCmd will create the Cobra command for detailing with files from a Labradar.
func NewLabradarCmd(cfg *config.Config) *cobra.Command {
	var labradarDirectory = ""

	lbrCmd := &cobra.Command{
		Use:              "labradar <command>",
		Short:            "Manage Labradar data files.",
		Long:             "View, upload, and describe the Labradar series.",
		TraverseChildren: true,
	}

	lbrCmd.PersistentFlags().StringVarP(&labradarDirectory, LbrDirectoryFlagParam, "d", "", "The directory holding the LBR files.")
	err := lbrCmd.MarkPersistentFlagRequired(LbrDirectoryFlagParam)
	if err != nil {
		logrus.Warnf("Could not make the persistent flag '%s' required.", LbrDirectoryFlagParam)
	}

	var lbrDirProvider = func() string { return config.AbsPathify(labradarDirectory) }

	lbrCmd.AddCommand(readseries.NewCmdRead(cfg, lbrDirProvider))
	lbrCmd.AddCommand(listseries.NewListLbrFilesCmd(cfg, lbrDirProvider))
	lbrCmd.AddCommand(describeseries.NewDescribeSeriesCmd(cfg, lbrDirProvider))

	return lbrCmd
}
