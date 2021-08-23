package commands

import (
	"fmt"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbattribute"
	"github.com/aws/aws-sdk-go/service/dynamodb/expression"
	"github.com/spf13/cobra"
	"log"
	"opgenorth.net/labradar/pkg/model"
	"sort"
)

func ListCartridgesCmd() *cobra.Command {

	cmd := &cobra.Command{
		Use:   "listcartridges",
		Short: "List the cartridges in Amplify",
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			return initializeConfig(cmd)
		},
		Run: func(cmd *cobra.Command, args []string) {
			for _, c := range getCartridgesFromAWS() {
				fmt.Println(c.ToString())
			}
		},
	}

	return cmd
}

func getCartridgesFromAWS() []model.Cartridge {
	// Initialize a session that the SDK will use to load
	// credentials from the shared credentials file ~/.aws/credentials
	// and region from the shared configuration file ~/.aws/config.
	sess := session.Must(session.NewSessionWithOptions(
		session.Options{
			SharedConfigState: session.SharedConfigEnable,
		}))

	// Create DynamoDB client
	svc := dynamodb.New(sess)

	proj := expression.NamesList(
		expression.Name("id"),
		expression.Name("name"),
		expression.Name("size"),
	)

	expr, err := expression.NewBuilder().
		WithProjection(proj).
		Build()
	if err != nil {
		log.Fatalf("Got error building expression: %s", err)
	}

	// Build the query input parameters
	params := &dynamodb.ScanInput{
		ExpressionAttributeNames:  expr.Names(),
		ExpressionAttributeValues: expr.Values(),
		FilterExpression:          expr.Filter(),
		ProjectionExpression:      expr.Projection(),
		TableName:                 aws.String("Cartridge-ns5rcz7k7jgbfhizt4qmyecvhy-staging"),
	}

	// Make the DynamoDB Query API call
	result, err := svc.Scan(params)
	if err != nil {
		log.Fatalf("Query API call failed: %s", err)
	}

	list := []model.Cartridge{}
	for _, i := range result.Items {
		item := model.Cartridge{}
		err = dynamodbattribute.UnmarshalMap(i, &item)
		if err != nil {
			log.Fatalf("Got error unmarshalling: %s", err)
		}
		list = append(list, item)
	}

	sort.Slice(list[:], func(i, j int) bool {
		return list[i].Name < list[j].Name
	})
	return list
}
