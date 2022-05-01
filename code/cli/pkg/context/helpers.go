package context

import (
	"os"
	"strings"
	"time"
)

// getEnviron will return a map of all environment variables.
func getEnviron() map[string]string {
	environ := map[string]string{}
	for _, env := range os.Environ() {
		envParts := strings.SplitN(env, "=", 2)
		key := envParts[0]
		value := ""
		if len(envParts) > 1 {
			value = envParts[1]
		}
		environ[key] = value
	}
	return environ
}

func InferTimeZone() string {
	t := time.Now()
	//zone, _ := t.Zone() // try to get my time zone...
	//loc, _ := time.LoadLocation(zone)
	tz := t.Location().String()
	return tz
}
