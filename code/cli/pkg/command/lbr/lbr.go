// Package lbr holds the code to build the cobra.Command structs for interacting with Labradar CSV files.
package lbr

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"github.com/spf13/cobra"
	listseries "opgenorth.net/mylittlerangebook/pkg/command/lbr/listseries"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/fs"
	"os"
	"path/filepath"
	"strings"
)

const (
	LbrDirectoryFlagParam = "lbr_dir"
)

// NewLabradarCmd will create the Cobra command for detailing with files from a Labradar.
func NewLabradarCmd(cfg *config.Config) *cobra.Command {
	defaultLbrDir, err := getDefaultLbrDirectory(cfg.Filesystem)
	var lbrDir = ""
	if err != nil {
		defaultLbrDir = ""
		logrus.WithError(err).Warnf("There was a problem trying to guess a default LBR folder; hopefully one is provided.")
	} else {
		if len(defaultLbrDir) == 0 {
			logrus.Warn("There is no LBR folder in the directory; hopefully one is provided.")
		} else {
			logrus.Tracef("Will use %s as the default directory if one is not provided.", defaultLbrDir)
		}
	}

	lbrCmd := &cobra.Command{
		Use:              "labradar <command>",
		Short:            "Manage Labradar data files.",
		Long:             "View, upload, and describe the Labradar series.",
		TraverseChildren: true,
	}

	usageMsg := "The directory holding the LBR series."
	lbrCmd.PersistentFlags().StringVarP(&lbrDir, LbrDirectoryFlagParam, "d", defaultLbrDir, usageMsg)

	lbrDirFn := func() string {
		fullPath := fs.AbsPathify(lbrDir)
		if !strings.HasSuffix(fullPath, "LBR") {
			fullPath = filepath.Join(fullPath, "LBR")
		}
		return fs.AbsPathify(fullPath)
	}
	addSubcommands(lbrCmd, cfg, lbrDirFn)

	return lbrCmd
}

func addSubcommands(parentCmd *cobra.Command, cfg *config.Config, defaultLbrDirFn func() string) {

	//parentCmd.AddCommand(readseries.NewCmdRead(cfg, defaultLbrDirFn))
	parentCmd.AddCommand(listseries.NewListLbrFilesCmd(cfg, defaultLbrDirFn))
	//parentCmd.AddCommand(describeseries.NewDescribeSeriesCmd(cfg, defaultLbrDirFn))
}

func getDefaultLbrDirectory(fs *afero.Afero) (string, error) {
	path, err := os.Executable()
	if err != nil {
		return "", err
	}

	exePath := filepath.Join(filepath.Dir(path), "LBR")

	exists, err := fs.Exists(exePath)
	if err != nil {
		return "", err
	}

	if exists {
		return exePath, nil
	}

	return "", nil
}
