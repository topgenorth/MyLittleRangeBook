package list

import (
	"fmt"
	"github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"io"
	"opgenorth.net/mylittlerangebook/pkg/config"
	"opgenorth.net/mylittlerangebook/pkg/mlrb"
	"path/filepath"
	"strings"
)

type LabradarListCommandOptions struct {
	// LabradarDirectory is a func that will return the path to the Labradar directory.
	LabradarDirectory func() string // TODO [TO20220122] Duplication with read.LabradarDirectoryProvider
}

// NewListLbrFilesCmd will create the Cobra command to list all the CSV files in an LBR directory.
func NewListLbrFilesCmd(cfg *config.Config, lbrDirProvider func() string) *cobra.Command {

	opts := &LabradarListCommandOptions{
		LabradarDirectory: lbrDirProvider,
	}

	c := &cobra.Command{
		Use:   "list",
		Short: "Will display a list of all Labaradar series ih the Labradar folder..",
		RunE: func(cmd *cobra.Command, args []string) error {
			return listLabradarSeries(cfg, opts)
		},
	}

	return c
}

func listLabradarSeries(cfg *config.Config, opts *LabradarListCommandOptions) error {
	a := mlrb.New(cfg)
	dir := opts.LabradarDirectory()
	files, err := a.GetListOfLabradarFiles(dir)
	if err != nil {
		return err
	}

	printFilenamesToStdOut(cfg.Out, dir, files)
	return nil
}

func printFilenamesToStdOut(w io.Writer, lbrDir string, files []string) {
	_, err := fmt.Fprintln(w, fmt.Sprintf("There are %d series in %s", len(files), lbrDir))
	if err != nil {
		logrus.WithError(err).Errorln("Can't display the list of files.")
		return
	}

	for _, f := range files {
		_, err := fmt.Fprintln(w, fmt.Sprintf(" %s", getSeriesNameFromPath(f)))
		if err != nil {
			logrus.WithError(err).Warnf("Ignore the filename %s", f)
		}
	}

	_, _ = fmt.Fprintln(w, "")
}

func getSeriesNameFromPath(path string) string {
	basename := filepath.Base(path)
	noExtension := strings.TrimSuffix(basename, filepath.Ext(basename))
	seriesName := strings.Split(noExtension, " ")[0]

	return seriesName
}
