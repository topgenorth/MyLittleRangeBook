package util

import (
	"time"
)

const defaultDateLayout = "yyyy-MM-dd"

func NewTimeProvider(dateFormat string) TimeProvider {

	if dateFormat == "" {
		return realTimeProvider{DateLayout: defaultDateLayout}
	}
	return realTimeProvider{
		defaultDateLayout,
	}
}

func NewMockTimeProvider(date time.Time, dateFormat string) TimeProvider {

	p := mockTimeProvider{FixedTime: date}
	if dateFormat == "" {
		p.DateLayout = defaultDateLayout
	} else {
		p.DateLayout = dateFormat
	}
	return p
}

type TimeProvider interface {
	Now() time.Time
	String() string
}
type realTimeProvider struct {
	DateLayout string
}

func (r realTimeProvider) String() string {
	return r.Now().Format(r.DateLayout)
}

func (r realTimeProvider) Now() time.Time {
	return time.Now()
}

type mockTimeProvider struct {
	DateLayout string
	FixedTime  time.Time
}

func (m mockTimeProvider) String() string {
	return m.FixedTime.Format(m.DateLayout)
}

func (m mockTimeProvider) Now() time.Time {
	return m.FixedTime
}
