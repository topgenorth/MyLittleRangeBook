package context

import (
	"bytes"
	"fmt"
	"github.com/pkg/errors"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"github.com/stretchr/testify/require"
	"io"
	"io/ioutil"
	"opgenorth.net/mylittlerangebook/pkg/test"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

const (
	TestPrefix     = "mlrb"
	TempDirForTest = "mlrb-test"
)

type TestContext struct {
	*AppContext

	cleanupDirs []string
	capturedErr *bytes.Buffer
	capturedOut *bytes.Buffer
	T           *testing.T
	afs         *afero.Afero

	TestDir string
}

// NewTestContext initializes a configuration suitable for testing, with the
// output buffered, and an in-memory file system, using the specified
// environment variables.
func NewTestContext(t *testing.T) *TestContext {
	// Provide a way for tests to provide and capture stdin and stdout
	// Copy output to the test log simultaneously, use go test -v to see the output
	err := &bytes.Buffer{}
	aggErr := io.MultiWriter(err, test.Logger{T: t})
	out := &bytes.Buffer{}
	aggOut := io.MultiWriter(out, test.Logger{T: t})

	appContext := &AppContext{
		Debug:      true,
		environ:    getEnviron(),
		Filesystem: &afero.Afero{Fs: afero.NewMemMapFs()},
		In:         &bytes.Buffer{},
		Out:        aggOut,
		Err:        aggErr,
		Timezone:   InferTimeZone(),
	}
	c := &TestContext{
		AppContext:  appContext,
		capturedOut: out,
		capturedErr: err,
		T:           t,
		afs:         &afero.Afero{Fs: appContext.Filesystem},
	}

	return c
}

func (c *TestContext) GetTestDefinitionDirectory() string {
	for i := 0; true; i++ {
		_, filename, _, ok := runtime.Caller(i)
		if !ok {
			c.T.Fatal("could not determine calling test directory")
		}
		filename = strings.ToLower(filename)
		if strings.HasSuffix(filename, "_test.go") {
			return filepath.Dir(filename)
		}
	}
	return ""
}

// UseFilesystem has AppContext use the OS filesystem instead of an in-memory filesystem
// Returns the test directory, and the temp mlrb home directory.
func (c *TestContext) UseFilesystem() (testDir string, homeDir string) {
	homeDir, err := ioutil.TempDir("", TempDirForTest)
	require.NoError(c.T, err)
	c.TestDir = c.GetTestDefinitionDirectory()

	c.cleanupDirs = append(c.cleanupDirs, homeDir)
	c.cleanupDirs = append(c.cleanupDirs, c.TestDir)

	return testDir, homeDir
}

func (c *TestContext) AddCleanupDir(dir string) {
	c.cleanupDirs = append(c.cleanupDirs, dir)
}

func (c *TestContext) Cleanup() {
	for _, dir := range c.cleanupDirs {

		err := c.Filesystem.RemoveAll(dir)
		if err != nil {
			logrus.Warnf("Could not delete the directory %s: %v", dir, err)
		}
	}
}

// AddTestFile will add a new file to the testing context.
// mode is optional and only the first one passed is used.
func (c *TestContext) AddTestFile(src, dest string, mode ...os.FileMode) []byte {
	data, err := ioutil.ReadFile(src)
	if err != nil {
		c.T.Fatal(errors.Wrapf(err, "error reading file %s from host filesystem", src))
	}

	var perms os.FileMode
	if len(mode) == 0 {
		ext := filepath.Ext(dest)
		if ext == ".sh" || ext == "" {
			perms = 0700
		} else {
			perms = 0600
		}
	} else {
		perms = mode[0]
	}

	err = c.afs.WriteFile(dest, data, perms)
	if err != nil {
		c.T.Fatal(errors.Wrapf(err, "error writing file %s to test filesystem", dest))
	}

	return data
}

func (c *TestContext) AddTestFileContents(file []byte, dest string) error {
	return c.afs.WriteFile(dest, file, 0600)
}

// mode is optional and should only be specified once
func (c *TestContext) AddTestDirectory(srcDir, destDir string, mode ...os.FileMode) {
	err := filepath.Walk(srcDir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		// Skip the root src directory
		if path == srcDir {
			return nil
		}

		// Translate the path from the src to the final destination
		dest := filepath.Join(destDir, strings.TrimPrefix(path, srcDir))

		if info.IsDir() {
			return c.afs.MkdirAll(dest, 0700)
		}

		c.AddTestFile(path, dest, mode...)
		return nil
	})
	if err != nil {
		c.T.Fatal(err)
	}
}

func (c *TestContext) AddTestDriver(src, name string) string {
	data, err := ioutil.ReadFile(src)
	if err != nil {
		c.T.Fatal(err)
	}

	dirname, err := c.afs.TempDir("", TestPrefix)
	if err != nil {
		c.T.Fatal(err)
	}

	// filename in accordance with cnab-go's command driver expectations
	filename := fmt.Sprintf("%s/cnab-%s", dirname, name)

	newfile, err := c.afs.Create(filename)
	if err != nil {
		c.T.Fatal(err)
	}

	if len(data) > 0 {
		_, err := newfile.Write(data)
		if err != nil {
			c.T.Fatal(err)
		}
	}

	err = c.afs.Chmod(newfile.Name(), 0700)
	if err != nil {
		c.T.Fatal(err)
	}
	err = newfile.Close()
	if err != nil {
		c.T.Fatal(err)
	}

	path := c.GetEnv("PATH")
	pathlist := []string{dirname, path}
	newpath := strings.Join(pathlist, string(os.PathListSeparator))
	c.SetEnv("PATH", newpath)

	return dirname
}

// GetOutput returns all text printed to stdout.
func (c *TestContext) GetOutput() string {
	return string(c.capturedOut.Bytes())
}

// GetError returns all text printed to stderr.
func (c *TestContext) GetError() string {
	return string(c.capturedErr.Bytes())
}

func (c *TestContext) ClearOutputs() {
	c.capturedOut.Truncate(0)
	c.capturedErr.Truncate(0)
}

// FindRepoRoot returns the path to the mlrb repository where the test is currently running
func (c *TestContext) FindRepoRoot() string {
	goMod := c.findRepoFile("go.mod")
	return filepath.Dir(goMod)
}

// FindBinDir returns the path to the bin directory of the repository where the test is currently running
func (c *TestContext) FindBinDir() string {
	return c.findRepoFile("bin")
}

// Finds a file in the mlrb repository, does not use the mock filesystem
func (c *TestContext) findRepoFile(wantFile string) string {
	d := c.GetTestDefinitionDirectory()
	for {
		if foundFile, ok := c.hasChild(d, wantFile); ok {
			return foundFile
		}

		d = filepath.Dir(d)
		if d == "." || d == "" || d == filepath.Dir(d) {
			c.T.Fatalf("could not find %s", wantFile)
		}
	}
}

func (c *TestContext) hasChild(dir string, childName string) (string, bool) {
	children, err := ioutil.ReadDir(dir)
	if err != nil {
		c.T.Fatal(err)
	}
	for _, child := range children {
		if child.Name() == childName {
			return filepath.Join(dir, child.Name()), true
		}
	}
	return "", false
}
