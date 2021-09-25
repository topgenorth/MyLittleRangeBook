package labradar

import (
	"bufio"
	"fmt"
	"opgenorth.net/labradar/pkg/config"
	"path/filepath"
	"strconv"
	"strings"
)

type ReadmeLine struct {
	LineNumber   int
	SeriesNumber int
	Firearm      *Firearm
	Load         *LoadData
	Text         string
	Err          error
}

func LoadReadmeFromFile(cfg *config.Config) ([]*ReadmeLine, error) {
	filename := filepath.Join(cfg.InputDir, "README.md")

	file, err := openFile(filename, cfg.Context.Filesystem)
	if err != nil {
		return nil, fmt.Errorf("Could not load the file %s. %s", filename, err)
	}
	defer closeFile(file)

	var lineNumber = 0
	var lines = []*ReadmeLine{}

	s := bufio.NewScanner(file)
	for s.Scan() {
		if lineNumber > 7 {
			t := s.Text()
			line := getReadmeLine(t)

			if line.Err == nil {
				line.LineNumber = lineNumber
				lines = append(lines, line)
			} else {
				fmt.Printf("Could not parse line %d: %s. %s\n", lineNumber, t, line.Err)
			}
		}

		lineNumber++
	}

	if err := s.Err(); err != nil {
		return nil, err
	}

	return lines, nil
}

func getReadmeLine(text string) *ReadmeLine {

	t := strings.TrimSpace(text)
	var err error = nil
	parts := strings.Split(t, "|")[1:5]

	seriesNumber, err := strconv.Atoi(strings.TrimSpace(parts[0]))
	ld := getLoadData(parts[1])
	f := &Firearm{strings.TrimSpace(parts[2]), ld.Cartridge}

	r := &ReadmeLine{
		LineNumber:   0,
		SeriesNumber: seriesNumber,
		Firearm:      f,
		Load:         ld,
		Text:         t,
		Err:          err,
	}

	return r
}

func getLoadData(ammoPart string) *LoadData {
	ammoParts := strings.Split(ammoPart, ";")

	ld := &LoadData{
		Cartridge:  strings.TrimSpace(ammoParts[0]),
		Projectile: getProjectileFrom(ammoParts[1]),
		Powder:     getPowderChargeFrom(ammoParts[2]),
	}

	return ld

}

func getProjectileFrom(s string) *Projectile {
	p := &Projectile{
		Name:   strings.TrimSpace(s),
		Weight: 0,
		BC:     nil,
	}
	return p
}

func getPowderChargeFrom(s string) *PowderCharge {
	pc := &PowderCharge{
		Name:   strings.TrimSpace(s),
		Amount: 0,
	}

	return pc
}
