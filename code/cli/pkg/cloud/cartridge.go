package cloud

import (
	"bytes"
	"context"
	"fmt"
	"gocloud.dev/docstore"
	"io"
	"text/template"
)

const tmpl_tostring_cartridge = `{{.Name}} ({{.Size}}) [{{.Id}}]`
const tbl_name_cartridge = "Cartridge-ns5rcz7k7jgbfhizt4qmyecvhy-staging"
var  empty_cartridge = Cartridge{
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


func fetchAll() {

}


type  collStuff struct {
	url string
}


func FetchAllCartridges() ([]Cartridge, error) {

	c := &collStuff{
		"dynamodb://"+tbl_name_cartridge+"?partition_key=id",

	}

	ctx := context.Background()
	coll, err := docstore.OpenCollection(ctx, "dynamodb://"+tbl_name_cartridge+"?partition_key=id")
	if err != nil {
		return nil, fmt.Errorf("FetchAllCartridges %v.", err)
	}
	defer func(coll *docstore.Collection) {
		_ = coll.Close()
	}(coll)

	list := []Cartridge{}
	iter := coll.Query().Get(ctx)
	defer iter.Stop()

	for {
		row := map[string]interface{}{}
		err := iter.Next(ctx, row)
		if err == io.EOF {
			break
		} else if err != nil {
			return nil, err
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

