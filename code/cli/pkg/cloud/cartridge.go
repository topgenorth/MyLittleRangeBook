package cloud

import (
	"bytes"
	"context"
	"fmt"
	"gocloud.dev/docstore"
	_ "gocloud.dev/docstore/awsdynamodb"
	"io"
	"text/template"
)

const tmpl_tostring_cartridge = `{{.Name}} ({{.Size}}) [{{.Id}}]`
const tbl_name_cartridge = "Cartridge-ns5rcz7k7jgbfhizt4qmyecvhy-staging"

var empty_cartridge = Cartridge{
	Id:      "",
	Name:    "",
	Size:    "",
	Version: 0,
}

type Cartridge struct {
	Id      string
	Name    string
	Size    string
	Version int64
}

func (c *Cartridge) IsEmpty() bool {
	return c.Id == "" && c.Name == "" && c.Size == "" && c.Version == 0
}

func (c Cartridge) String() string {
	t, err := template.New("Cartridge").Parse(tmpl_tostring_cartridge)
	if err != nil {
		panic(err)
	}

	var buf bytes.Buffer
	if err := t.Execute(&buf, c); err != nil {
		panic(err)
	}

	return buf.String()
}

type awsCartridgeCollection struct {
	Url  string
	Ctx  context.Context
	iter *docstore.DocumentIterator
}

func (c *awsCartridgeCollection) getIterator() (*docstore.DocumentIterator, error) {

	coll, err := docstore.OpenCollection(c.Ctx, c.Url)
	if err != nil {
		return nil, fmt.Errorf("getIterator - opening the collection: %v", err)
	}
	defer func(coll *docstore.Collection) {
		_ = coll.Close()
	}(coll)

	iter := coll.Query().Get(c.Ctx)
	defer iter.Stop()

	return iter, nil
}

func FetchAllCartridges() ([]Cartridge, error) {

	c := &awsCartridgeCollection{
		"dynamodb://" + tbl_name_cartridge + "?partition_key=id",
		context.Background(),
		nil,
	}

	iter, err := c.getIterator()
	if err != nil {
		return nil, fmt.Errorf("FetchAllCartridges: %v", err)
	}

	list := []Cartridge{}
	for {
		row := map[string]interface{}{}
		err := iter.Next(c.Ctx, row)
		if err == io.EOF {
			break
		} else if err != nil {
			return nil, fmt.Errorf("FetchAllCartridges: %v", err)
		} else {
			list = append(list, Cartridge{
				Id:      row["id"].(string),
				Name:    row["name"].(string),
				Size:    row["size"].(string),
				Version: row["_version"].(int64),
			})
		}
	}

	return list, nil
}
