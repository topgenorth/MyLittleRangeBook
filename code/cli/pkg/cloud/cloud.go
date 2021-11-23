package cloud

import (
	"fmt"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"gocloud.dev/blob"
	_ "gocloud.dev/blob/s3blob"
	_ "gocloud.dev/docstore/awsdynamodb"
	"golang.org/x/net/context"
	"io"
	"io/ioutil"
	"path/filepath"
)

const S3_LABRADAR_CSV_BUCKET_FOR_INCOMING_FILES = "mylittlerangebookca841fe0a51d4b32898841aff4e72b25240-staging"
const AWS_REGION = "us-east-1"

type labradarCsvFile struct {
	*aws.Config
	BucketName string
	Key        string
	SourceFile string
	Bytes      []byte
}

func buildIncomingLabradarConfig(filename string) (*labradarCsvFile, error) {
	csvBytes, err := ioutil.ReadFile(filename)
	if err != nil {
		return nil, fmt.Errorf("buildIncomingLabradarConfig %v", err)
	}

	csv := &labradarCsvFile{
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
		return fmt.Errorf("getAwsSession - error trying to read the incoming CSV file %s: %v", filename, err)
	}

	ctx := context.Background()

	s3Bucket, err := blob.OpenBucket(ctx, "s3://"+csvFile.BucketName)
	if err != nil {
		return fmt.Errorf("getAwsSession - opening S3 bucket: %v", err)
	}
	defer func(s3Bucket *blob.Bucket) {
		_ = s3Bucket.Close()
	}(s3Bucket)

	w, err := blob.PrefixedBucket(s3Bucket, "incoming").NewWriter(ctx, "/"+csvFile.Key, nil)
	if err != nil {
		return fmt.Errorf("getAwsSession - opening S3 bucket: %v", err)
	}
	_, err = w.Write(csvFile.Bytes)

	if err != nil {
		return fmt.Errorf("getAwsSession - trying to write the bytes: %v", err)
	}
	if err := w.Close(); err != nil {
		return fmt.Errorf("getAwsSession - return to close the Writer: %v", err)
	}
	return nil
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

