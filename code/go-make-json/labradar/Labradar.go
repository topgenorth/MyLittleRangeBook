package labradar

type Labradar struct {
	DeviceId   string `json:"deviceId"`
	Date       string
	Time       string
	SeriesName string          `json:"seriesName"`
	Units      *UnitsOfMeasure `json:"units"`
	Stats      *Velocities     `json:"stats"`
}
