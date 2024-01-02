package catalog

import (
	"fmt"
	"github.com/pkg/errors"
	"github.com/rs/zerolog"
	"github.com/spf13/afero"
	"github.com/spf13/cobra"
	"github.com/spf13/viper"
	constants "labreader/internal"
	"labreader/internal/logger"
	"labreader/pkg/timeprovider"
	"path/filepath"
)

const dateLayoutForFilePrefix = "20060102"

var (
	afs *afero.Afero
	l   zerolog.Logger
)

func NewCatalogCommand() *cobra.Command {
	catalogCmd := &cobra.Command{
		Use:   "catalog",
		Short: "Move the file to the specified directory.",
		Args:  cobra.ExactArgs(1),
		Long: `The file will be copied to the Dropbox directory, in the prescribed directory structure.

All of the meta-data for the file must be specified on
the command line.  The rifle is mandatory.`,
		RunE: moveFileToDropbox,
	}

	return configCliFlags(catalogCmd)
}

func init() {
	fs := afero.NewOsFs()
	afs = &afero.Afero{Fs: fs}
	l = logger.New(logger.DevelopmentEnvironment(), logger.LogLevelDebug())
}

//goland:noinspection GoUnusedParameter
func moveFileToDropbox(cmd *cobra.Command, args []string) error {
	meta := commandLineValues{}
	err := viper.Unmarshal(&meta)
	if err != nil {
		return errors.Wrap(err, "invalid meta data to add the file to the catalog")
	}

	dir := destinationDirectory(meta)
	// TODO [TO20231212] DI for TimeProvider
	t := timeprovider.New(timeprovider.WithDateLayout(dateLayoutForFilePrefix))
	destination := filepath.Join(dir, timestampDestinationFile(args[0], t))

	if meta.Dryrun {
		l.
			Info().
			Str("dryrun", fmt.Sprintf("Would move the file %s to %s", args[0], destination)).
			Str("cmdline", meta.String()).
			Send()
		return nil
	}

	err = moveFile(afs, args[0], destination)
	if err != nil {
		return errors.Wrap(err, "could not move the file")
	}

	l.Info().
		Str("source", args[0]).
		Str("destination", destination).
		Send()

	return nil
}

func configCliFlags(catalogCmd *cobra.Command) *cobra.Command {
	catalogCmd.PersistentFlags().StringP("catalog-folder", "", "", "The path to the folder that holds the catalog files.")
	_ = catalogCmd.MarkFlagRequired("catalog-folder")
	_ = viper.BindEnv("LBR_CATALOG_FOLDER")

	catalogCmd.PersistentFlags().StringP("rifle", "", "", "The name of the relevant rifle.")
	_ = viper.BindPFlag("rifle", catalogCmd.PersistentFlags().Lookup("rifle"))
	_ = catalogCmd.MarkFlagRequired("rifle")

	catalogCmd.PersistentFlags().StringP("cartridge", "", constants.UnknownStr, "The name of the cartridge.")
	_ = viper.BindPFlag("cartridge", catalogCmd.PersistentFlags().Lookup("cartridge"))
	viper.SetDefault("cartridge", constants.UnknownStr)

	catalogCmd.PersistentFlags().StringP("powder", "", constants.UnknownStr, "The name of the gun powder that was used.")
	_ = viper.BindPFlag("powder", catalogCmd.PersistentFlags().Lookup("powder"))
	viper.SetDefault("powder", constants.UnknownStr)

	catalogCmd.PersistentFlags().Float32P("powder-charge", "", 0, "The weight of the gun powder.")
	_ = viper.BindPFlag("powder-charge", catalogCmd.PersistentFlags().Lookup("powder-charge"))
	viper.SetDefault("powder-charge", 0)

	catalogCmd.PersistentFlags().StringP("bullet", "", constants.UnknownStr, "The name of the bullet that was used.")
	_ = viper.BindPFlag("bullet", catalogCmd.PersistentFlags().Lookup("bullet"))
	viper.SetDefault("bullet", constants.UnknownStr)

	catalogCmd.PersistentFlags().Int32P("bullet-weight", "", 0, "The weight of the bullet.")
	_ = viper.BindPFlag("bullet-weight", catalogCmd.PersistentFlags().Lookup("bullet-weight"))
	viper.SetDefault("bullet-weight", 0)

	catalogCmd.PersistentFlags().Float32P("coal", "", 0, "Cartridge Over All Length (inches)")
	_ = viper.BindPFlag("coal", catalogCmd.PersistentFlags().Lookup("coal"))
	viper.SetDefault("coal", 0)

	catalogCmd.PersistentFlags().Float32P("cbto", "", 0, "Cartridge Base To Overall length (inches)")
	_ = viper.BindPFlag("cbto", catalogCmd.PersistentFlags().Lookup("cbto"))
	viper.SetDefault("cbto", 0)

	catalogCmd.PersistentFlags().BoolP("rename", "", false, "Will rename the file instead of moving it.")
	_ = viper.BindPFlag("rename", catalogCmd.PersistentFlags().Lookup("rename"))
	viper.SetDefault("rename", false)

	return catalogCmd
}
