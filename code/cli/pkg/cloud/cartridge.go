package cloud

import (
	"bytes"
	"context"
	"fmt"
	"github.com/google/uuid"
	"gocloud.dev/docstore"
	_ "gocloud.dev/docstore/awsdynamodb"
	"io"
	"text/template"
)

const tmpl_tostring_cartridge = `{{.Name}} ({{.Size}}) [{{.Id}}]`
const tbl_name_cartridge = "Cartridge-5yebjmcc75d6tp6w2hvr54b6b4-staging"

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

type addCartridgeCmd struct {
	id   string
	name string
	size string
}

type cartridgeCollection struct {
	*docstore.Collection
	url     string
	ctx     context.Context
	results []Cartridge
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

// AddCartridge will insert the specified cartridge into the database.  It will return
// the version of the Cartridge that is in the database at the time of the upsert.
func AddCartridge(name string, size string) (*Cartridge, error) {
	var (
		ctx = context.Background()
		url = "dynamodb://" + tbl_name_cartridge + "?partition_key=id"
	)

	// Open the collection
	coll, err := docstore.OpenCollection(ctx, url)
	if err != nil {
		return nil, fmt.Errorf("AddCartridge failed trying to open the collection - %v", err)
	}
	defer func(coll *docstore.Collection) {
		_ = coll.Close()
	}(coll)

	doc := &addCartridgeCmd{uuid.New().String(), name, size}
	if err := coll.Put(ctx, doc); err != nil {
		return nil, fmt.Errorf("AddCartridge failed trying to insert the data - %v", err)
	}

	c := &Cartridge{}
	return c, nil
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
