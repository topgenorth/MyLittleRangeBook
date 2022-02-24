package persistence

import (
	"fmt"
	"github.com/google/uuid"
	"gorm.io/gorm"
	"time"
)

const (
	TomsLabradarDevice = "Labradar0013797"
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

func (s SeriesGORM) New() *SeriesBuilder {
	return &SeriesBuilder{
		device: TomsLabradarDevice,
		date:   time.Now(),
	}
}
func (s SeriesGORM) Save(series Series) {
	var tx *gorm.DB
	if series.ID < 1 {
		tx = s.db.Create(&series)
	} else {
		tx = s.db.Save(&series)
	}

	if tx.Error != nil {
		s.RecentErr = handleSqlite3Error(s.RecentErr)
	} else {
		s.RecentErr = nil
	}
}
func (s SeriesGORM) GetAll() []Series {
	var series []Series
	if result := s.db.Order("date").Find(&series); result.Error != nil {
		s.RecentErr = handleSqlite3Error(result.Error)
		return nil
	}
	return series
}
func (s SeriesGORM) DeleteById(id uint) {
	tx := s.db.Delete(&Series{}, id)
	s.RecentErr = handleSqlite3Error(tx.Error)
}
func (s SeriesGORM) Delete(series *Series) {
	if result := s.db.Delete(series); result.Error != nil {
		s.RecentErr = result.Error
	}
}

type SeriesBuilder struct {
	device        string
	name          string
	date          time.Time
	firearm       string
	ammo          string
	notes         string
	average       uint
	max           uint
	min           uint
	stddev        float64
	extremespread uint
}

func (sb *SeriesBuilder) Device(d string) *SeriesBuilder {
	sb.device = d
	return sb
}
func (sb *SeriesBuilder) Name(name string) *SeriesBuilder {
	sb.name = name
	return sb
}
func (sb *SeriesBuilder) Date(date time.Time) *SeriesBuilder {
	sb.date = date
	return sb
}
func (sb *SeriesBuilder) Firearm(f string) *SeriesBuilder {
	sb.firearm = f
	return sb
}
func (sb *SeriesBuilder) Ammo(a string) *SeriesBuilder {
	sb.ammo = a
	return sb
}
func (sb *SeriesBuilder) Notes(n string) *SeriesBuilder {
	sb.notes = n
	return sb
}
func (sb *SeriesBuilder) Average(a uint) *SeriesBuilder {
	sb.average = a
	return sb
}
func (sb *SeriesBuilder) Min(m uint) *SeriesBuilder {
	sb.min = m
	return sb
}
func (sb *SeriesBuilder) Max(m uint) *SeriesBuilder {
	sb.max = m
	return sb
}
func (sb *SeriesBuilder) StdDev(sd float64) *SeriesBuilder {
	sb.stddev = sd
	return sb
}
func (sb *SeriesBuilder) ExtremeSpread(es uint) *SeriesBuilder {
	sb.extremespread = es
	return sb
}
func (sb *SeriesBuilder) Build() Series {
	return Series{
		Device:        sb.device,
		Name:          sb.name,
		Date:          sb.date,
		Firearm:       sb.firearm,
		Ammo:          sb.ammo,
		Notes:         sb.notes,
		Average:       sb.average,
		Max:           sb.max,
		Min:           sb.min,
		StdDev:        sb.stddev,
		ExtremeSpread: sb.extremespread,
		UUID:          uuid.New(),
	}
}
