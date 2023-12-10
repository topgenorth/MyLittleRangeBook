package context

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestContext_EnvironMap(t *testing.T) {
	c := NewTestContext(t)
	c.ClearEnv()

	c.SetEnv("a", "1")
	c.SetEnv("b", "2")

	got := c.EnvironMap()

	want := map[string]string{
		"a": "1",
		"b": "2",
	}
	assert.Equal(t, want, got)

	// Make sure we have a copy
	got["c"] = "3"
	assert.Empty(t, c.GetEnv("c"), "Expected to get a copy of the context's environment variables")
}
