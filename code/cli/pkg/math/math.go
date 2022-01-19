package math

import "math"

// GetMinAndMaxForInts will return two values, the minimum value and the maximum value in an array of integers.
func GetMinAndMaxForInts(array []int) (int, int) {
	max := array[0]
	min := array[0]
	for _, value := range array {
		if max < value {
			max = value
		}
		if min > value {
			min = value
		}
	}
	return min, max
}

// CalculateAverageOfInts will take an array of integers and calculate the average value (as a float64).
func CalculateAverageOfInts(array []int) float64 {
	count := len(array)
	sum := 0
	for _, velocity := range array {
		sum += velocity
	}

	value := float64(sum) / float64(count)
	valRounded := math.Round(value*10) / 10
	return valRounded
}

// CalculateStdDevOfInts will calculate the standard deviation for an array of integers (as a float 64).
func CalculateStdDevOfInts(array []int) float64 {
	count := len(array)
	mean := CalculateAverageOfInts(array)
	var sd float64
	sd = float64(0)
	for j := 0; j < count; j++ {
		sd += math.Pow(float64(array[j])-mean, 2)
	}

	value := math.Sqrt(sd / float64(count))
	valRounded := math.Round(value*10) / 10
	return valRounded
}

// IsNumericOnly will return true if all the characters in a string are numeric.
func IsNumericOnly(str string) bool {

	if len(str) == 0 {
		return false
	}

	for _, s := range str {
		if s < '0' || s > '9' {
			return false
		}
	}
	return true
}
