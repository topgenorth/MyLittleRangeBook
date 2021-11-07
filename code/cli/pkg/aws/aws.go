package aws

import (
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
)


func buildAwsConfig() *aws.Config {
	return &aws.Config{
		Region: aws.String("us-east-1"),
	}
}

func GetSession() *session.Session {
	sess, err := session.NewSession(buildAwsConfig())
	if err != nil {
		panic(err)
	}
	return sess
}

