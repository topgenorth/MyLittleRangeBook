package labradar

import (
	"fmt"
	"opgenorth.net/tomutil/mathhelpers"
)

// velocityData holds a list of velocities.
type velocityData struct {
	Values []int `json:"values"`
}

func (vd *velocityData) CountOfShots() int {
	return len(vd.Values)
}
func (vd *velocityData) String() string {
	return fmt.Sprintf("%d velocities with an average of %d", len(vd.Values), vd.Average())
}

// Append will append the new velocity value to the set.
func (vd *velocityData) Append(velocity int) {
	vd.Values = append(vd.Values, velocity)
}

// StdDev will return the standard deviation for a set of velocityData.
func (vd *velocityData) StdDev() float64 {
	return mathhelpers.CalculateStdDevOfInts(vd.Values)
}

// emptyVelocityData will initialize an empty velocityData struct.
func emptyVelocityData() *velocityData {
	v := &velocityData{Values: nil}
	return v
}

// ExtremeSpread will return the extreme spread for a set of velocityData.
func (vd *velocityData) ExtremeSpread() int {
	smallest, largest := mathhelpers.GetMinAndMaxForInts(vd.Values)
	return largest - smallest
}

// Average will return the average for a set of velocityData.
func (vd *velocityData) Average() int {
	return int(mathhelpers.CalculateAverageOfInts(vd.Values))
}

// Min will return the minimum velocity in a set of velocityData.
func (vd *velocityData) Min() int {
	_, smallest := mathhelpers.GetMinAndMaxForInts(vd.Values)
	return smallest
}

// Max will return the maximum velocity in a set of velocityData.
func (vd *velocityData) Max() int {
	largest, _ := mathhelpers.GetMinAndMaxForInts(vd.Values)
	return largest
}
