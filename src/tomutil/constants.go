// Package tomutil holds the sundry bits and bobs that I think might be shareable in different modules.
package tomutil

const (
	// TomTimeZone is the timezone is the default timezone if we can't infer/determine the timezone of the device.
	// We use America/Edmonton because I'm in Edmonton, AB.  :)
	TomTimeZone = "America/Edmonton"
	// UnicodeNUL is the Unicode representation of a NULL.
	UnicodeNUL = "\u0000"
	// HexNUL is the Unicode representation of a NULL.
	HexNUL = "\x00"
	// UnknownStr is a magic string.
	UnknownStr = "<UNKNOWN>"

	DevelopmentEnvironment RuntimeEnvironment = "DEVELOPMENT"
	ProductionEnvironment  RuntimeEnvironment = "PRODUCTION"
)

type RuntimeEnvironment string
