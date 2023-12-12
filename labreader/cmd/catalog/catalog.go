package catalog

import (
	"fmt"
	"github.com/pkg/errors"
	"github.com/spf13/afero"
	"github.com/spf13/cobra"
	"github.com/spf13/viper"
	"labreader/internal/logger"
	"labreader/internal/util"
	"path/filepath"
	"time"
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

	return initCommand(catalogCmd)
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
	destination := filepath.Join(dir, timestampDestinationFile(args[0], RealTimeProvider{}))

	if meta.Dryrun {
		logger.DefaultLogger().
			Info().
			Str("dryrun", fmt.Sprintf("Would move the file %s to %s", args[0], destination)).
			Str("cmdline", meta.String()).
			Send()
		return nil
	}

	// TODO [TO20231212] DI for *afero.Afero
	fs := afero.NewOsFs()
	afs := &afero.Afero{Fs: fs}
	err = moveFile(afs, args[0], destination)
	if err != nil {
		return errors.Wrap(err, "could not move the file")
	}

	logger.DefaultLogger().Info().
		Str("source", args[0]).
		Str("destination", destination).
		Send()

	return nil
}

/*--- */

func initCommand(catalogCmd *cobra.Command) *cobra.Command {

	catalogCmd.PersistentFlags().StringP("rifle", "", "", "The name of the relevant rifle.")
	_ = viper.BindPFlag("rifle", catalogCmd.PersistentFlags().Lookup("rifle"))
	_ = catalogCmd.MarkFlagRequired("rifle")

	catalogCmd.PersistentFlags().StringP("cartridge", "", util.UnknownStr, "The name of the cartridge.")
	_ = viper.BindPFlag("cartridge", catalogCmd.PersistentFlags().Lookup("cartridge"))
	viper.SetDefault("cartridge", util.UnknownStr)

	catalogCmd.PersistentFlags().StringP("powder", "", util.UnknownStr, "The name of the gun powder that was used.")
	_ = viper.BindPFlag("powder", catalogCmd.PersistentFlags().Lookup("powder"))
	viper.SetDefault("powder", util.UnknownStr)

	catalogCmd.PersistentFlags().Float32P("powder-charge", "", 0, "The weight of the gun powder.")
	_ = viper.BindPFlag("powder-charge", catalogCmd.PersistentFlags().Lookup("powder-charge"))
	viper.SetDefault("powder-charge", 0)

	catalogCmd.PersistentFlags().StringP("bullet", "", util.UnknownStr, "The name of the bullet that was used.")
	_ = viper.BindPFlag("bullet", catalogCmd.PersistentFlags().Lookup("bullet"))
	viper.SetDefault("bullet", util.UnknownStr)

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

type TimeProvider interface {
	Now() time.Time
	String() string
}
type RealTimeProvider struct{}

func (r RealTimeProvider) String() string {
	return r.Now().Format(dateLayout)
}

func (r RealTimeProvider) Now() time.Time {
	return time.Now()
}

type MockTimeProvider struct {
	FixedTime time.Time
}

func (m MockTimeProvider) String() string {
	return m.FixedTime.Format(dateLayout)
}

func (m MockTimeProvider) Now() time.Time {
	return m.FixedTime
}
