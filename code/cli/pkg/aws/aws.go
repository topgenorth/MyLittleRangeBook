package aws

import (
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/s3/s3manager"
	"os"
	"path/filepath"
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

func AddFileToS3(s *session.Session, filename string) error {

	file, err := os.Open(filename)
	if err != nil {
		return err
	}

	defer file.Close()

	uploader := s3manager.NewUploader(s)

	key := aws.String("incoming/" +filepath.Base(filename))

	_, err = uploader.Upload(&s3manager.UploadInput{
		Bucket: aws.String("mylittlerangebookca841fe0a51d4b32898841aff4e72b25240-staging"),
		Key: key,
		Body: file,
	})

	if err != nil {
		return err
	}

	return nil
}
