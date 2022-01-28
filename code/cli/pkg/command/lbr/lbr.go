// Package lbr holds the code to build the cobra.Command structs for interacting with Labradar CSV files.
package lbr

import (
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"github.com/spf13/cobra"
	describeseries "opgenorth.net/mylittlerangebook/pkg/command/lbr/describe"
	listseries "opgenorth.net/mylittlerangebook/pkg/command/lbr/list"
	readseries "opgenorth.net/mylittlerangebook/pkg/command/lbr/read"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"os"
	"path/filepath"
)

const (
	LbrDirectoryFlagParam = "lbr_dir"
)

// NewLabradarCmd will create the Cobra command for detailing with files from a Labradar.
func NewLabradarCmd(cfg *config.Config) *cobra.Command {
	defaultLbrDir, err := getDefaultLbrDirectory(cfg.Filesystem)
	var lbrDir string = ""
	if err != nil {
		logrus.WithError(err).Warnf("There was a problem trying to guess a default LBR folder; hopefully one is provided.")
	} else {
		if len(defaultLbrDir) == 0 {
			logrus.Warn("There is no LBR folder in the directory; hopefully one is provided.")
		} else {
			logrus.Tracef("Will use %s as the default directory if one is not provided.")
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

	addSubcommands(lbrCmd, cfg, func() string { return config.AbsPathify(lbrDir) })

	return lbrCmd
}

func addSubcommands(parentCmd *cobra.Command, cfg *config.Config, lbrDir func() string) {

	parentCmd.AddCommand(readseries.NewCmdRead(cfg, lbrDir))
	parentCmd.AddCommand(listseries.NewListLbrFilesCmd(cfg, lbrDir))
	parentCmd.AddCommand(describeseries.NewDescribeSeriesCmd(cfg, lbrDir))
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
