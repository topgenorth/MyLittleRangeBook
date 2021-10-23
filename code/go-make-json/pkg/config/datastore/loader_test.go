package datastore

import (
	"opgenorth.net/labradar/pkg/config"
	"os"
	"testing"

	"github.com/spf13/cobra"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func TestFromConfigFile(t *testing.T) {
	c := config.NewTestConfig(t)
	c.SetHomeDir("/root/.mlrb")

	c.TestContext.AddTestFile("testdata/config.toml", "/root/.mlrb/config.toml")

	c.DataLoader = FromConfigFile
	err := c.LoadData()
	require.NoError(t, err, "dataloader failed")
	require.NotNil(t, c.Data, "config.Data was not populated")
	assert.True(t, c.Debug, "config.Debug was not set correctly")
}

func TestFromFlagsThenEnvVarsThenConfigFile(t *testing.T) {
	// Cannot be run in parallel because viper reads directly from env vars
	buildCommand := func(c *config.Config) *cobra.Command {
		cmd := &cobra.Command{}
		cmd.Flags().BoolVar(&c.Debug, "debug", false, "debug")
		cmd.PreRunE = func(cmd *cobra.Command, args []string) error {
			return c.LoadData()
		}
		cmd.RunE = func(cmd *cobra.Command, args []string) error {
			return nil
		}
		c.DataLoader = FromFlagsThenEnvVarsThenConfigFile(cmd)
		return cmd
	}

	t.Run("no flag", func(t *testing.T) {
		c := config.NewTestConfig(t)
		c.SetHomeDir("/root/.mlrb")

		cmd := buildCommand(c.Config)
		err := cmd.Execute()
		require.NoError(t, err, "dataloader failed")
		require.NotNil(t, c.Data, "config.Data was not populated")
		assert.False(t, c.Debug, "config.Debug was not set correctly")
	})

	t.Run("debug flag", func(t *testing.T) {
		c := config.NewTestConfig(t)
		c.SetHomeDir("/root/.mlrb")

		cmd := buildCommand(c.Config)
		cmd.SetArgs([]string{"--debug"})
		err := cmd.Execute()

		require.NoError(t, err, "dataloader failed")
		require.NotNil(t, c.Data, "config.Data was not populated")
		assert.True(t, c.Debug, "config.Debug was not set correctly")
	})

	t.Run("debug flag overrides config", func(t *testing.T) {
		c := config.NewTestConfig(t)
		c.SetHomeDir("/root/.mlrb")
		c.TestContext.AddTestFile("testdata/config.toml", "/root/.mlrb/config.toml")

		cmd := buildCommand(c.Config)
		cmd.SetArgs([]string{"--debug=false"})
		err := cmd.Execute()

		require.NoError(t, err, "dataloader failed")
		require.NotNil(t, c.Data, "config.Data was not populated")
		assert.False(t, c.Debug, "config.Debug should have been set by the flag and not the config")
	})

	t.Run("debug env var", func(t *testing.T) {
		os.Setenv("MLRB_DEBUG", "true")
		c := config.NewTestConfig(t)
		c.SetHomeDir("/root/.mlrb")

		cmd := buildCommand(c.Config)
		err := cmd.Execute()

		require.NoError(t, err, "dataloader failed")
		require.NotNil(t, c.Data, "config.Data was not populated")
		assert.True(t, c.Debug, "config.Debug was not set correctly")
	})

	t.Run("invalid debug env var", func(t *testing.T) {
		os.Setenv("MLRB_DEBUG", "blorp")
		c := config.NewTestConfig(t)
		c.SetHomeDir("/root/.mlrb")

		cmd := buildCommand(c.Config)
		err := cmd.Execute()

		require.NoError(t, err, "dataloader failed")
		require.NotNil(t, c.Data, "config.Data was not populated")
		assert.False(t, c.Debug, "config.Debug was not set correctly")
	})

	t.Run("debug env var overrides config", func(t *testing.T) {
		os.Setenv("MLRB_DEBUG", "false")
		defer os.Unsetenv("MLRB_DEBUG")
		c := config.NewTestConfig(t)
		c.SetHomeDir("/root/.MLRB")
		c.TestContext.AddTestFile("testdata/config.toml", "/root/.mlrb/config.toml")

		cmd := buildCommand(c.Config)
		err := cmd.Execute()

		require.NoError(t, err, "dataloader failed")
		require.NotNil(t, c.Data, "config.Data was not populated")
		assert.False(t, c.Debug, "config.Debug should have been set by the env var and not the config")
	})

	t.Run("flag overrides debug env var overrides config", func(t *testing.T) {
		os.Setenv("MLRB_DEBUG", "false")
		defer os.Unsetenv("MLRB_DEBUG")

		c := config.NewTestConfig(t)
		c.SetHomeDir("/root/.mlrb")
		c.TestContext.AddTestFile("testdata/config.toml", "/root/.mlrb/config.toml")

		cmd := buildCommand(c.Config)
		cmd.SetArgs([]string{"--debug", "true"})
		err := cmd.Execute()

		require.NoError(t, err, "dataloader failed")
		require.NotNil(t, c.Data, "config.Data was not populated")
		assert.True(t, c.Debug, "config.Debug should have been set by the flag and not the env var or config")
	})
}

func TestData_Marshal(t *testing.T) {
	c := config.NewTestConfig(t)
	c.SetHomeDir("/root/.mlrb")

	c.TestContext.AddTestFile("testdata/config.toml", "/root/.mlrb/config.toml")

	c.DataLoader = FromConfigFile
	err := c.LoadData()
	require.NoError(t, err, "LoadData failed")

	require.NotNil(t, c.Data, "Data was not populated by LoadData")

	require.Equal(t, c.Data.Labradar.InputDir, "/root/labradar/LBR")
	require.Equal(t, c.Data.Labradar.OutputDir, "/root/labradar/json")

}
