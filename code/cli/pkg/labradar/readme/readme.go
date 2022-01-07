package readme

import (
	"bufio"
	"fmt"
	"github.com/carolynvs/aferox"
	"github.com/spf13/afero"
	"opgenorth.net/mylittlerangebook/pkg/labradar"
	"os"
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

func (r *ReadmeMd) AppendSeries(s labradar.Series) {
	i := len(r.lines)
	str := ""
	r.lines = append(r.lines, &readmeLine{index: i, value: str})
}

func Save(r ReadmeMd, fs aferox.Aferox) error {

	if len(r.lines) > 1 {
		sort.SliceStable(r.lines, func(i, j int) bool {
			return r.lines[i].index < r.lines[j].index
		})
	}

	file, err := fs.OpenFile(r.Filename, 0644, os.ModeExclusive)
	defer func(f afero.File) {
		_ = f.Close()
	}(file)
	if err != nil {
		return fmt.Errorf("could not open the README.MD %s:%w", r.Filename, err)
	}

	writer := bufio.NewWriter(file)
	defer func(w *bufio.Writer) {
		_ = w.Flush()
	}(writer)

	for i, line := range r.lines {
		_, err := writer.WriteString(line.value + "\n")
		if err != nil {
			return fmt.Errorf("problem writing to the file %s, line number %d: %w", r.Filename, i, err)
		}
	}

	return nil
}
