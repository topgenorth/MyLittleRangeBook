package persistence

import (
	"fmt"
	"github.com/google/uuid"
	"gorm.io/gorm"
	"time"
)

type Series struct {
	gorm.Model
	Device        string    `gorm:"index:idx_series__device,not null,index:idx_series__devicename,unique,not null,priority:1"`
	Name          string    `gorm:"index:idx_series__name,not null;index:idx_series__devicename,priority:2"`
	Date          time.Time `gorm:"index:idx_series__date,not null"`
	Firearm       string    `gorm:"index:idx_series__firearm,not null"`
	Ammo          string    `gorm:"index:idx_series__ammo,not null"`
	Notes         string    `gorm:"index:idx_series__notes"`
	Average       uint      `gorm:"check,>-1"`
	Max           uint      `gorm:"check,>-1"`
	Min           uint      `gorm:"check,>-1"`
	StdDev        float64   `gorm:"check,>=0.0"`
	ExtremeSpread uint      `gorm:"check,>-1"`
	UUID          uuid.UUID `gorm:"index:idx_series__uuid,unique,not null"`
}

func (t Series) String() string {
	return fmt.Sprintf("%s (%s)", t.Name, t.Device)
}

type SeriesGORM struct {
	db        *gorm.DB
	RecentErr error
}

func (s SeriesGORM) New() *Series {
	series := &Series{
		Date: time.Now(),
		UUID: uuid.New(),
	}

	return series
}
