package cartridge

import (
	"context"
	"errors"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"gocloud.dev/docstore/awsdynamodb"
	"io"
	"opgenorth.net/mylittlerangebook/pkg/aws"
	"strings"
)

const CARTRIDGE_TABLENAME = "Cartridge"

type Cartridge struct {
	Id               string
	Name             string
	Size             string
	DocstoreRevision interface{}
}

func (c *Cartridge) ToString() string {
	var sb strings.Builder

	sb.WriteString(c.Id)
	sb.WriteString(": ")
	sb.WriteString(c.Name)
	sb.WriteString(" (")
	sb.WriteString(c.Size)
	sb.WriteString(")")

	return sb.String()

}

func Save(c Cartridge) error {
	return errors.New("Not implemented.")
}

func loadFrom(m map[string]interface{}) Cartridge {
	c := Cartridge{
		Id:   m["id"].(string),
		Name: m["name"].(string),
		Size: m["size"].(string),
	}
	return c
}

func FetchAll() ([]Cartridge, error) {

	// TODO Need to clean this up; lots of hardcoded AWS stuff.
	sess, err := session.NewSession()
	if err != nil {
		return nil, err
	}

	coll, err := awsdynamodb.OpenCollection(
		dynamodb.New(sess), aws.GetTableName(CARTRIDGE_TABLENAME), "id", "", nil)
	if err != nil {
		return nil, err
	}
	defer coll.Close()

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
