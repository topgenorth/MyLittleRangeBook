package labradar

import (
	"strconv"
	"time"
)

// Series is a structure that holds the data from a Labradar series, and some details of the load
// and firearm that was used.
type Series struct {
	// TODO [TO20220404] Maybe this should be an interface?
	Number         SeriesNumber
	deviceId       DeviceId
	Date           string
	Time           string
	Velocities     *VelocityData
	Firearm        *Firearm
	LoadData       *LoadData
	Notes          string
	UnitsOfMeasure *UnitsOfMeasure
}

// SeriesMutatorFn describes a function that can be used to manipulate the values of a Series
type SeriesMutatorFn = func(s *Series)

func (s Series) String() string {
	return s.Number.String()
}

func (s Series) DeviceId() DeviceId {
	return s.deviceId
}

// CountOfShots will retrieve the number of shots in the series.
func (s Series) CountOfShots() int {
	return s.Velocities.CountOfShots()
}

// Update will use the provided mutators to update values in the Series
func (s *Series) Update(mutators ...SeriesMutatorFn) {
	for _, mutate := range mutators {
		mutate(s)
	}
}

// NewSeries will take a collection of SeriesMutatorFn functions, create a new Series, and then
// update it accordingly.
func NewSeries(mutators ...SeriesMutatorFn) *Series {

	s := &Series{
		Number:     0,
		Velocities: emptyVelocityData(),
		Firearm: &Firearm{
			Name:      "",
			Cartridge: "",
		},
		LoadData:       emptyLoadData(),
		UnitsOfMeasure: emptyUnitsOfMeasure(),
		Notes:          "",
		Date:           "",
		Time:           "",
	}

	s.Update(mutators...)
	return s
}

func TryParseSeriesNumber(sr string) (SeriesNumber, bool) {
	if len(sr) != 6 {
		return SeriesNumber(0), false
	}
	if sr[0:2] != "SR" {
		return SeriesNumber(0), false
	}

	i, err := strconv.Atoi(sr[2:6])
	if err != nil {
		return 0, false
	}

	return SeriesNumber(i), true
}

// UpdateDeviceForSeries will update the series.Series with the device id of the specified device.
func UpdateDeviceForSeries(device *DeviceDirectory) SeriesMutatorFn {
	// TODO [TO20220119] Needs unit tests
	return func(s *Series) {
		s.deviceId = device.DeviceId()
	}
}

// WithSeriesNumber will initialize the number assigned by a specific Labradar device
func WithSeriesNumber(n int) SeriesMutatorFn {
	return func(s *Series) {
		s.Number = SeriesNumber(n)
	}
}

// WithFirearm will set the cartridge and name of the firearm.  This does not update the cartridge on the LoadData
func WithFirearm(name string) SeriesMutatorFn {
	return func(s *Series) {
		s.Firearm.Name = name
	}
}

// WithCartridge will set the cartridge of the LoadData. This does update the cartridge on the Firearm.
func WithCartridge(cartridge string) SeriesMutatorFn {
	// TODO [TO20220123] What should we do if the cartridge cartridge doesn't match the cartridge on the firearm?
	return func(s *Series) {
		s.Firearm.Cartridge = cartridge
		s.LoadData.Cartridge = cartridge
	}
}

// WithNotes will update the notes field.
func WithNotes(notes string) SeriesMutatorFn {
	return func(s *Series) {
		s.Notes = notes
	}
}

// WithPowder will set the name of the gunpowder used and weight on the PowderCharge.
func WithPowder(name string, weight float32) SeriesMutatorFn {
	return func(s *Series) {
		s.LoadData.Powder.Name = name
		s.LoadData.Powder.Amount = weight
	}
}

// WithProjecticle will set the name and weight of the projectile on the Projectile.
func WithProjecticle(name string, weight int) SeriesMutatorFn {
	return func(s *Series) {
		s.LoadData.Projectile.Name = name
		s.LoadData.Projectile.Weight = weight
	}
}

// UsingGrainsForWeight sets the units of measure to grains.
func UsingGrainsForWeight() SeriesMutatorFn {
	return func(s *Series) {
		s.UnitsOfMeasure.Weight = "gr"
	}
}

// UsingCurrentDateAndTime will use the time from this process as the default.
func UsingCurrentDateAndTime() SeriesMutatorFn {
	return func(s *Series) {
		now := time.Now()
		s.Date = now.Format("2006-01-02")
		s.Time = now.Format("15:04")
	}
}

// UsingFeetPerSecondForMuzzleVelocity will set the default velocity units to FPS
func UsingFeetPerSecondForMuzzleVelocity() SeriesMutatorFn {
	return func(s *Series) {
		s.UnitsOfMeasure.Velocity = "fps"
	}
}

// UsingMetresPerSecondForMuzzleVelocity will set the default velocity units to m/s
func UsingMetresPerSecondForMuzzleVelocity() SeriesMutatorFn {
	return func(s *Series) {
		s.UnitsOfMeasure.Velocity = "m/s"
	}
}

// UsingYardsForDistance will set the default distance units to yards.
func UsingYardsForDistance() SeriesMutatorFn {
	return func(s *Series) {
		s.UnitsOfMeasure.Distance = "y"
	}
}

// UsingMetresForDistance will set the default distance units to metres.
func UsingMetresForDistance() SeriesMutatorFn {
	return func(s *Series) {
		s.UnitsOfMeasure.Distance = "m"
	}
}

// UsingFeetForDistance will set the default distance units to feet.
func UsingFeetForDistance() SeriesMutatorFn {
	return func(s *Series) {
		s.UnitsOfMeasure.Distance = "ft"
	}
}

// LabradarSeriesDefaults returns the mutators that will set default values on a Series.
func LabradarSeriesDefaults() []SeriesMutatorFn {
	defaults := []SeriesMutatorFn{
		UsingGrainsForWeight(),
		UsingMetresForDistance(),
		UsingFeetPerSecondForMuzzleVelocity(),
		UsingCurrentDateAndTime(),
		UsingCelsiusForTemperature(),
	}

	return defaults
}

// UsingCelsiusForTemperature will return a mutator to set the temperature Units of Measure to Centigrade.
func UsingCelsiusForTemperature() SeriesMutatorFn {
	return func(s *Series) {
		s.UnitsOfMeasure.Temperature = "Celsius"
	}
}

// UsingFarenheitForTemperature will return a mutator to set the temperature Units of Measure to Fahrenheit.
func UsingFarenheitForTemperature() SeriesMutatorFn {
	return func(s *Series) {
		s.UnitsOfMeasure.Temperature = "Fahrenheit"
	}
}

// combineMutators will combine two separate arrays of SeriesMutatorFn into one.  The items in the first
// array will appear first.
func combineMutators(first []SeriesMutatorFn, second []SeriesMutatorFn) []SeriesMutatorFn {
	mutators := make([]SeriesMutatorFn, len(first)+len(second))
	index := 0

	for i := 0; i < len(first); i++ {
		mutators[index] = first[i]
		index++
	}

	for i := 0; i < len(second); i++ {
		mutators[index] = second[i]
		index++
	}

	return mutators
}
