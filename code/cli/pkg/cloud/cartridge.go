package cloud

import (
	"bytes"
	"context"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"gocloud.dev/docstore"
	"gocloud.dev/docstore/awsdynamodb"
	"io"
	"text/template"
)

type Cartridge struct {
	Id      string
	Name    string
	Size    string
	Version int64
}

func (c *Cartridge) IsEmpty() bool {
	return  c.Id == "" && c.Name == "" && c.Size == "" && c.Version == 0
}

func (c Cartridge) ToString() string {
	t, err := template.New("Series").Parse(tmpl)
	if err != nil {
		panic(err)
	}

	var buf bytes.Buffer
	if err := t.Execute(&buf, c); err != nil {
		panic(err)
	}

	return buf.String()
}

func emptyCartridge() Cartridge {
	return Cartridge{
		Id:      "",
		Name:    "",
		Size:    "",
		Version: 0,
	}
}
func loadFrom(m map[string]interface{}) Cartridge {
	c := Cartridge{
		Id:      m["id"].(string),
		Name:    m["name"].(string),
		Size:    m["size"].(string),
		Version: m["_version"].(int64),
	}
	return c
}


func UpsertCartridge(c Cartridge) Cartridge {
	return emptyCartridge()
}

func FetchAllCartridges() ([]Cartridge, error) {

	// TODO [TO20211109] Replace with GoCloud.dev stuff

	sess := GetSession()

	coll, err := awsdynamodb.OpenCollection(
		dynamodb.New(sess), "Cartridge-ns5rcz7k7jgbfhizt4qmyecvhy-staging", "id", "", nil)
	if err != nil {
		return nil, err
	}

	defer func(coll *docstore.Collection) {
		_ = coll.Close()
	}(coll)

	ctx := context.Background()

	iter := coll.Query().Get(ctx)
	defer iter.Stop()

	list := []Cartridge{}
	for {
		m := map[string]interface{}{}
		err := iter.Next(ctx, m)
		if err == io.EOF {
			break
		} else if err != nil {
			return nil, err
		} else {
			list = append(list, loadFrom(m))
		}
	}

	return list, nil
}

const tmpl = `{{.Name}} ({{.Size}}) [{{.Id}}]`
