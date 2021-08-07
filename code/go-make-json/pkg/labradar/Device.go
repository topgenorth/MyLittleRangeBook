package labradar

type Device struct {
	DeviceId   string          `json:"deviceId"`
	Date       string          `json:"date"`
	Time       string          `json:"time"`
	TimeZone   string          `json:"timezone"`
	SeriesName string          `json:"seriesName"`
	Units      *UnitsOfMeasure `json:"units"`
	Stats      *Velocities     `json:"stats"`
}
