// Package lbr holds the code to build the cobra.Command structs for interacting with Labradar CSV files.
package lbr

import (
	"github.com/spf13/cobra"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/fs"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"os"
	"path/filepath"
	"strings"
)

const (
	LbrDirectoryFlagParam = "lbr_dir"
)

// NewLabradarCmd will create the Cobra command for detailing with files from a Labradar.
func NewLabradarCmd(cfg *config.Config) *cobra.Command {
	var lbrDir = ""

	lbrCmd := &cobra.Command{
		Use:              "lbr",
		Short:            "Manage Labradar data files.",
		Long:             "View, upload, and describe the Labradar series.",
		TraverseChildren: true,
	}
	usageMsg := "The directory holding the LBR series."
	lbrCmd.PersistentFlags().StringVarP(&lbrDir, LbrDirectoryFlagParam, "d", inferDefaultLabradarDirectory(), usageMsg)

	defaultLbrDirectoryFn := func() string {
		fullPath := fs.AbsPathify(lbrDir)
		if !strings.HasSuffix(fullPath, "LBR") {
			fullPath = filepath.Join(fullPath, "LBR")
		}
		return fs.AbsPathify(fullPath)
	}
	addSubcommands(lbrCmd, cfg, defaultLbrDirectoryFn)

	return lbrCmd
}

func addSubcommands(parentCmd *cobra.Command, cfg *config.Config, defaultLbrDirFn labradar.DirectoryProviderFn) {
	parentCmd.AddCommand(NewSummarizeSeriesCmd(cfg, defaultLbrDirFn))
	parentCmd.AddCommand(NewListLbrFilesCmd(cfg, defaultLbrDirFn))
	parentCmd.AddCommand(NewDescribeSeriesCmd(cfg, defaultLbrDirFn))
}

func inferDefaultLabradarDirectory() string {
	path, err := os.Executable()
	if err != nil {
		return labradar.DefaultLabradarDir()

	}
	return filepath.Join(filepath.Dir(path), labradar.DefaultLabradarDir())
}
