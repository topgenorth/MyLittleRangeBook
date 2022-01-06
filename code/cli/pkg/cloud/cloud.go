package cloud

import (
	"fmt"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"gocloud.dev/blob"
	_ "gocloud.dev/blob/s3blob"
	_ "gocloud.dev/docstore/awsdynamodb"
	"golang.org/x/net/context"
	"io/ioutil"
	"path/filepath"
)

const S3_LABRADAR_CSV_BUCKET_FOR_INCOMING_FILES = "mylittlerangebookca841fe0a51d4b32898841aff4e72b25240-staging"
const AWS_REGION = "us-east-1"

// This struct holds a Labradar CSV file to submit to S3.
type labradarS3File struct {
	*aws.Config
	BucketName string
	Key        string
	SourceFile string
	Bytes      []byte
}

func buildIncomingLabradarConfig(filename string) (*labradarS3File, error) {
	csvBytes, err := ioutil.ReadFile(filename)
	if err != nil {
		return nil, fmt.Errorf("Could not build the LabradarS3File: %w", err)
	}

	csv := &labradarS3File{
		buildAwsConfig(),
		S3_LABRADAR_CSV_BUCKET_FOR_INCOMING_FILES,
		filepath.Base(filename),
		filename,
		csvBytes,
	}

	return csv, nil
}

func buildAwsConfig() *aws.Config {
	return &aws.Config{
		Region: aws.String(AWS_REGION),
	}
}

func getAwsSession() *session.Session {
	sess, err := session.NewSession(buildAwsConfig())
	if err != nil {
		panic(err)
	}
	return sess
}

func SubmitLabradarCsvFile(filename string) error {
	csvFile, err := buildIncomingLabradarConfig(filename)
	if err != nil {
		return fmt.Errorf("getAwsSession - error trying to read the incoming CSV file %s: %w", filename, err)
	}

	ctx := context.Background()

	s3Bucket, err := blob.OpenBucket(ctx, "s3://"+csvFile.BucketName)
	if err != nil {
		return fmt.Errorf("getAwsSession - opening S3 bucket: %w", err)
	}
	defer func(s3Bucket *blob.Bucket) {
		_ = s3Bucket.Close()
	}(s3Bucket)

	w, err := blob.PrefixedBucket(s3Bucket, "incoming").NewWriter(ctx, "/"+csvFile.Key, nil)
	if err != nil {
		return fmt.Errorf("getAwsSession - opening S3 bucket: %w", err)
	}
	_, err = w.Write(csvFile.Bytes)

	if err != nil {
		return fmt.Errorf("getAwsSession - trying to write the bytes: %w", err)
	}
	if err := w.Close(); err != nil {
		return fmt.Errorf("getAwsSession - could not close the Writer: %w", err)
	}
	return nil
}
