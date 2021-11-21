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
	Id            string
	Name          string
	Size          string
	Version       int64
	LastChangedAt int64
	UpdatedAt     string
	CreatedAt     string
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

type cartridgeCollection struct {
	*docstore.Collection
	url     string
	ctx     context.Context
	results []Cartridge
}

func FetchAllCartridges() ([]Cartridge, error) {

	cartridges, err := openCartridgeCollection()
	if err != nil {
		return nil, fmt.Errorf("could not open the dynamodb collection - %v", err)
	}

	iter := cartridges.Query().Get(cartridges.ctx)
	defer iter.Stop()

	for {
		row := map[string]interface{}{}
		err := iter.Next(cartridges.ctx, row)
		if err == io.EOF {
			break
		} else if err != nil {
			return nil, fmt.Errorf("there was a problem retrieving the cartridges: %v", err)
		} else {
			c := Cartridge{
				Id:            row["id"].(string),
				Name:          row["name"].(string),
				Size:          row["size"].(string),
				Version:       row["_version"].(int64),
				LastChangedAt: row["_lastChangedAt"].(int64),
				UpdatedAt:     row["updatedAt"].(string),
				CreatedAt:     row["createdAt"].(string),
			}
			cartridges.results = append(cartridges.results, c)
		}
	}

	return cartridges.results, nil
}

func openCartridgeCollection() (*cartridgeCollection, error) {
	ctx := context.Background()
	url := "dynamodb://" + tbl_name_cartridge + "?partition_key=id"
	coll, err := docstore.OpenCollection(ctx, url)
	if err != nil {
		return nil, err
	}
	cartridges := &cartridgeCollection{
		coll,
		url,
		context.Background(),
		[]Cartridge{},
	}
	return cartridges, nil
}
