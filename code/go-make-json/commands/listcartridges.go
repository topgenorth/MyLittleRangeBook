package commands

import (
	"context"
	"fmt"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/spf13/cobra"
	"gocloud.dev/docstore/awsdynamodb"
	_ "gocloud.dev/docstore/awsdynamodb"
	"io"
	"opgenorth.net/labradar/pkg/model"
	"sort"
)

const CARTRIDGE_TABLENAME = "Cartridge-ns5rcz7k7jgbfhizt4qmyecvhy-staging"

func ListCartridgesCmd() *cobra.Command {

	cmd := &cobra.Command{
		Use:   "listcartridges",
		Short: "List the cartridges in Amplify",
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			return initializeConfig(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {
			// cartridges = getCartridgesFromAWS()

			cartridges, err := getCartridges()
			if err != nil {
				fmt.Println("Problem retrieving a list of cartridges. ", err)
				return
			}
			displayCartridgesToStdOut(cartridges)
		},
	}

	return cmd
}

func displayCartridgesToStdOut(cartridges []model.Cartridge) {
	sort.Slice(cartridges[:], func(i, j int) bool {
		return cartridges[i].Name < cartridges[j].Name
	})

	for _, c := range cartridges {
		fmt.Println(c.ToString())
	}
}

func getCartridges() ([]model.Cartridge, error) {
	sess, err := session.NewSession()
	if err != nil {
		return nil, err
	}
	coll, err := awsdynamodb.OpenCollection(
		dynamodb.New(sess), CARTRIDGE_TABLENAME, "id", "", nil)
	if err != nil {
		return nil, err
	}
	defer coll.Close()

	ctx := context.Background()

	iter := coll.Query().Get(ctx)
	defer iter.Stop()

	list := []model.Cartridge{}
	for {
		m := map[string]interface{}{}
		err := iter.Next(ctx, m)

		if err == io.EOF {
			break
		} else if err != nil {
			return nil, err
		} else {
			c := model.Cartridge{
				Id:   m["id"].(string),
				Name: m["name"].(string),
				Size: m["size"].(string),
			}
			list = append(list, c)
		}
	}

	return list, nil
}
