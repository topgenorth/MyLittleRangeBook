package config

import (
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/context"
	"path/filepath"
	"testing"
)

type TestConfig struct {
	*Config
	TestContext *context.TestContext
}

// NewTestConfig initializes a configuration suitable for testing, with the output buffered, and an in-memory file system.
func NewTestConfig(t *testing.T) *TestConfig {
	cxt := context.NewTestContext(t)
	cfg := New()
	cfg.AppContext = cxt.AppContext
	tc := &TestConfig{
		Config:      cfg,
		TestContext: cxt,
	}
	tc.SetupUnitTest()
	return tc
}

// SetupUnitTest initializes the unit test filesystem with the supporting files in the MLRB)HOME directory.
func (c *TestConfig) SetupUnitTest() {
	// Set up the test MLRB home directory
	home := fmt.Sprintf("/root/.%s", context.TestPrefix)
	c.SetHomeDir(home)

	// Fake out the mlrb home directory
	_, _ = c.Filesystem.Create(filepath.Join(home, context.TestPrefix))
	_, _ = c.Filesystem.Create(filepath.Join(home, "runtimes", "mlrb-runtime"))

}

// SetupIntegrationTest initializes the filesystem with the supporting files in
// a temp MLRB_HOME directory.
func (c *TestConfig) SetupIntegrationTest() (testDir string, homeDir string) {
	testDir, homeDir = c.TestContext.UseFilesystem()
	c.SetHomeDir(homeDir)

	// Use the compiled mlrb binary in the test home directory,
	// and not the go test binary that is generated when we run integration tests.
	// This way when mlrb calls back to itself, e.g. for internal plugins,
	// it is calling the normal mlrb binary.
	c.SetMlrbPath(filepath.Join(homeDir, context.TestPrefix))

	// Copy bin dir contents to the home directory
	c.TestContext.AddTestDirectory(c.TestContext.FindBinDir(), homeDir, 0700)

	return testDir, homeDir
}
