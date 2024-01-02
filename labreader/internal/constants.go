package constants

// DefaultTimeZone is the timezone is the default timezone if we can't infer/determine the timezone of the device.
// We use America/Edmonton because I'm in Edmonton, AB.  :)
//
//goland:noinspection GoUnusedConst
const DefaultTimeZone = "America/Edmonton"

const (
	// UnicodeNUL is the Unicode representation of a NULL
	UnicodeNUL = "\u0000"
	// HexNUL is the Unicode representation of a NULL
	HexNUL = "\x00"
	// UnknownStr is a magic string.
	UnknownStr = "<UNKNOWN>"
)
