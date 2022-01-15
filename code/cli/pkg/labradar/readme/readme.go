package readme

import (
	"bufio"
	"bytes"
	"fmt"
	"github.com/carolynvs/aferox"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"sort"
)

type readmeLine struct {
	index int
	value string
}

func (l readmeLine) String() string {
	return l.value
}

// This structure holds the contents of a ReadMe file for the Labradar series.
type ReadmeMd struct {
	Filename string
	lines    []*readmeLine
}

// Create a new ReadMe file but with zero lines of content.
func New(filename string) *ReadmeMd {

	r := &ReadmeMd{Filename: filename, lines: make([]*readmeLine, 5)}
	r.lines[0] = &readmeLine{0, "# Description of Labradar series\n\n"}
	r.lines[1] = &readmeLine{1, "For ammo, stick with the format:\n"}
	r.lines[2] = &readmeLine{2, "`Cartridge; Bullet; Powder; COAL;Description`\n\n"}
	r.lines[3] = &readmeLine{3, "| Series Number | Ammo | Firearm | Date |\n"}
	r.lines[4] = &readmeLine{4, "| :---:         | :--- | :-----  | :---: |\n"}

	return r
}

func Load(filename string, fs aferox.Aferox) (*ReadmeMd, error) {
	file, err := fs.Open(filename)
	defer func(f afero.File) {
		_ = f.Close()
	}(file)

	if err != nil {
		return nil, fmt.Errorf("failed to open the README file %s: %w", filename, err)
	}

	r := &ReadmeMd{Filename: filename}
	scanner := bufio.NewScanner(file)
	i := 0
	for scanner.Scan() {
		r.lines = append(r.lines, &readmeLine{index: i, value: scanner.Text()})
		i = i + 1
	}

	return r, nil
}

func (r *ReadmeMd) AppendSeries(s labradar.Series, oldformat bool) {
	// [TO20220110] What happens if we duplicate a series number?

	w := &labradar.ReadMeSeriesWriter{OldFormat: oldformat}
	if err := w.Write(s); err != nil {
		logrus.Error("could not serialize the series.")
		return
	}

	i := len(r.lines)
	r.lines = append(r.lines, &readmeLine{index: i, value: w.Output})
}

func Save(r ReadmeMd, fs aferox.Aferox) error {

	if len(r.lines) > 1 {
		sort.SliceStable(r.lines, func(i, j int) bool {
			return r.lines[i].index < r.lines[j].index
		})
	}

	var b bytes.Buffer
	for _, line := range r.lines {
		b.WriteString(line.value)
		b.WriteString("\n")
	}
	err := fs.WriteFile(r.Filename, b.Bytes(), 0644)
	if err != nil {
		return fmt.Errorf("could not open the README.MD %s:%w", r.Filename, err)
	}

	return nil
}
