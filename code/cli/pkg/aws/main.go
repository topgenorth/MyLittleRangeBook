package aws

import (
	"fmt"
	"opgenorth.net/mylittlerangebook/pkg/config"
)

// Get the table name for the table in staging.
func getStagingTableName(table string, cfg config.Config) string {
	return fmt.Sprintf("%s-%s-staging", table, cfg.Getenv("STAGING_APP_ID") )
}
