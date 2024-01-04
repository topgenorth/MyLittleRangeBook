package labradar

import (
	"fmt"
	"golang.org/x/exp/maps"

	"opgenorth.net/tomutil/mathhelpers"
)

// velocityData holds a list of velocities.  It only supports adding, doesn't support deleting or clearing.
type velocityData struct {
	values map[int]int `json: "values""`
}

func (vd *velocityData) Get(index int) int { return vd.values[index] }
func (vd *velocityData) Values() []int     { return maps.Values(vd.values) }
func (vd *velocityData) CountOfShots() int {
	return len(vd.values)
}
func (vd *velocityData) String() string {
	return fmt.Sprintf("%d velocities with an average of %d", vd.CountOfShots(), vd.Average())
}

// Append will append the new velocity value to the set.
func (vd *velocityData) Append(velocity int) {
	var maxNumber = 0
	for maxNumber = range vd.values {
		break
	}
	vd.values[maxNumber] = velocity
}

// StdDev will return the standard deviation for a set of velocityData.
func (vd *velocityData) StdDev() float64 {
	return mathhelpers.CalculateStdDevOfInts(vd.Values())
}

// emptyVelocityData will initialize an empty velocityData struct.
func emptyVelocityData() *velocityData {
	v := &velocityData{values: make(map[int]int)}
	return v
}

// ExtremeSpread will return the extreme spread for a set of velocityData.
func (vd *velocityData) ExtremeSpread() int {
	smallest, largest := mathhelpers.GetMinAndMaxForInts(vd.Values())
	return largest - smallest
}

// Average will return the average for a set of velocityData.
func (vd *velocityData) Average() int {
	return int(mathhelpers.CalculateAverageOfInts(vd.Values()))
}

// Min will return the minimum velocity in a set of velocityData.
func (vd *velocityData) Min() int {
	_, smallest := mathhelpers.GetMinAndMaxForInts(vd.Values())
	return smallest
}

// Max will return the maximum velocity in a set of velocityData.
func (vd *velocityData) Max() int {
	largest, _ := mathhelpers.GetMinAndMaxForInts(vd.Values())
	return largest
}
