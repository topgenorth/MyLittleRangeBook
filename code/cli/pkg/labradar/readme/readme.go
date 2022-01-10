package readme

import (
	"bufio"
	"bytes"
	"fmt"
	"github.com/carolynvs/aferox"
	"github.com/sirupsen/logrus"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"opgenorth.net/mylittlerangebook/pkg/labradar/io"
	"sort"
)

type readmeLine struct {
	index int
	value string
}

func (l readmeLine) String() string {
	return l.value
}

type ReadmeMd struct {
	Filename string
	lines    []*readmeLine
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

	w := &io.ReadMeSeriesWriter{OldFormat: oldformat}
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
