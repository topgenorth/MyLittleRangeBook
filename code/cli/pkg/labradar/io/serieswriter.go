package io

import "opgenorth.net/mylittlerangebook/pkg/labradar"

type SeriesWriter interface {
	Write(s labradar.Series) error
}
