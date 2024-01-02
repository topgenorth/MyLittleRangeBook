// Package timeprovider is a wrapper around time.Now() so that we can mock out getting the time and to provide
// some flexibility around how the date is formatted.
package timeprovider

import (
	"time"
)

const defaultDateLayout = "yyyy-MM-dd"

var defaultNowFunc = func() time.Time { return time.Now() }

type TimeProvider struct {
	nowFunc    func() time.Time
	dateLayout string
}

// timeProviderOption is a function that will be used to setup/configure a TimeProvider instance.
type timeProviderOption func(*TimeProvider)

// New will create a TimeProvider configured according to the provided options.
func New(options ...timeProviderOption) *TimeProvider {

	tp := &TimeProvider{
		nowFunc:    defaultNowFunc,
		dateLayout: defaultDateLayout,
	}
	for _, o := range options {
		o(tp)
	}
	return tp
}

// Now will return the time as encapsulated by the provider.
func (t TimeProvider) Now() time.Time {
	return t.nowFunc()
}

// String will return the formatted the string.
func (t TimeProvider) String() string {
	s := t.Now().Format(t.dateLayout)
	return s
}

// WithDateLayout is a function that will accept a string that will be used to format the date.
func WithDateLayout(dateLayout string) func(*TimeProvider) {
	return func(provider *TimeProvider) {
		provider.dateLayout = dateLayout
	}
}

// WithDate will configure the TimeProvider to return a specific date.
func WithDate(fixedTime time.Time) func(*TimeProvider) {
	return func(provider *TimeProvider) {
		fn := func() time.Time {
			return fixedTime
		}
		provider.nowFunc = fn
	}
}
