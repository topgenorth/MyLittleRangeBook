package fs

import (
	"fmt"
	"github.com/spf13/afero"
	jww "github.com/spf13/jwalterweatherman"
	"os"
)

type fileParts struct {
	NameParts []string
	PathSep   string
	HomeDir   string
	LbrToken  string
}

func GetPathToLabradarSeries(seriesNumber int) string {

	var fileParts = &fileParts{
		[]string{"work", "labradar", "LBR"},
		string(os.PathSeparator),
		getHomeDir(),
		fmt.Sprintf("SR%04d", seriesNumber),
	}
	var pathToSeries = fileParts.HomeDir + fileParts.PathSep
	for _, part := range fileParts.NameParts {
		pathToSeries += part
		pathToSeries += fileParts.PathSep
	}
	pathToSeries += fileParts.LbrToken + fileParts.PathSep + fileParts.LbrToken + " Report.csv"
	return pathToSeries

}

func getHomeDir() string {
	homedir, err := os.UserHomeDir()
	if err != nil {
		jww.FATAL.Fatal(err)
	}
	return homedir
}

func readFile(filename string) (string, error) {
	a := afero.Afero{
		Fs: afero.NewOsFs(),
	}
	fileBytes, err := a.ReadFile(filename)
	if err != nil {

		return "", err
	}
	return string(fileBytes), nil
}
