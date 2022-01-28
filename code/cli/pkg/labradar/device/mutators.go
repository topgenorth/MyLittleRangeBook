package device

import "opgenorth.net/mylittlerangebook/pkg/labradar/series"

//UpdateDeviceForSeries will update the series.LabradarSeries with the device id of the specified device.
func UpdateDeviceForSeries(device *Device) series.LabradarSeriesMutatorFunc {
	// TODO [TO20220119] Needs unit tests
	return func(s *series.LabradarSeries) {
		s.DeviceId = device.DeviceId
	}
}
