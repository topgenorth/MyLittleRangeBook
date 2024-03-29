package cloud

import (
	"bytes"
	"context"
	"fmt"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbattribute"
	"github.com/google/uuid"
	"gocloud.dev/docstore"
	_ "gocloud.dev/docstore/awsdynamodb"
	"io"
	"text/template"
)

const TMPL_TOSTRING_CARTRIDGE = `{{.Name}} ({{.Size}}) [{{.Id}}]`
const tbl_name_cartridge = "Cartridge-5yebjmcc75d6tp6w2hvr54b6b4-staging"

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
	t, err := template.New("Cartridge").Parse(TMPL_TOSTRING_CARTRIDGE)
	if err != nil {
		panic(err)
	}

	var buf bytes.Buffer
	if err := t.Execute(&buf, c); err != nil {
		panic(err)
	}

	return buf.String()
}

// FetchAllCartridges will retrieve a list of all the cartridges in our list.
func FetchAllCartridges() ([]Cartridge, error) {

	cartridges, err := openCartridgeCollection()
	if err != nil {
		return nil, fmt.Errorf("could not open the dynamodb collection:  %w", err)
	}

	iter := cartridges.Query().Get(cartridges.ctx)
	defer iter.Stop()

	for {
		row := map[string]interface{}{}
		err := iter.Next(cartridges.ctx, row)
		if err == io.EOF {
			break
		} else if err != nil {
			return nil, fmt.Errorf("there was a problem retrieving the cartridges: %w", err)
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

type addCartridgeCmd struct {
	Id   string `json:"id"`
	Name string `json:"name"`
	Size string `json:"size"`
}

// AddCartridge will insert the specified cartridge into the database.  It will return
// the version of the Cartridge that is in the database at the time of the upsert.
func AddCartridge(name string, size string) (*Cartridge, error) {
	//var (
	//	ctx = context.Background()
	//	url = "dynamodb://" + tbl_name_cartridge + "?partition_key=id"
	//)

	// Marshal the values to an input item
	av, err := dynamodbattribute.MarshalMap(addCartridgeCmd{uuid.New().String(),
		name,
		size})
	if err != nil {
		return nil, fmt.Errorf("AddCartridge - couldn't marshal the values: %w", err)
	}
	fmt.Printf("marshalled struct: %+v", av)

	i := &dynamodb.PutItemInput{
		Item:      av,
		TableName: aws.String(tbl_name_cartridge),
	}

	// Initialize a session that the SDK will use to load
	// credentials from the shared credentials file ~/.aws/credentials
	// and region from the shared configuration file ~/.aws/config.
	sess := session.Must(session.NewSessionWithOptions(session.Options{SharedConfigState: session.SharedConfigEnable}))
	// Create DynamoDB client
	svc := dynamodb.New(sess)
	_, err = svc.PutItem(i)
	if err != nil {
		return nil, fmt.Errorf("AddCatridge - couldn't PUT the values: %w", err)
	}

	return nil, nil
}
