package aws

const STAGING_APP_ID = "ns5rcz7k7jgbfhizt4qmyecvhy"

func GetTableName(table string) string {
	return table + "-" + STAGING_APP_ID + "-staging"
}
