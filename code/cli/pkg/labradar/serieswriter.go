package labradar

// An interface to persisting a Series to something (HTML, JSON, Markdown, etc)
type SeriesWriter interface {
	Write(s Series) error
}
