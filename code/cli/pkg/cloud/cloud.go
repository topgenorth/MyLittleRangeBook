package cloud

import (
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"gocloud.dev/blob"
	_ "gocloud.dev/blob/s3blob"
	"golang.org/x/net/context"
	"io/ioutil"
	"path/filepath"
)

const S3_LABRADAR_CSV_BUCKET = "mylittlerangebookca841fe0a51d4b32898841aff4e72b25240-staging"

type IncomingLabradarCsv struct {
	*aws.Config
	BucketName string `json:"bucket"`
	Key        string `json:"key"`
	SourceFile string
}

func (b *IncomingLabradarCsv) IncomingBucket() string {
	return "s3://" + b.BucketName
}

func (b *IncomingLabradarCsv) ReadContents() ([]byte, error) {
	csvBytes, err := ioutil.ReadFile(b.SourceFile)
	if err != nil {
		return nil, err
	}
	return csvBytes, nil

}

func buildIncomingLabradarConfig(filename string) *IncomingLabradarCsv {
	return &IncomingLabradarCsv{
		buildAwsConfig(),
		S3_LABRADAR_CSV_BUCKET,
		"incoming/" + filepath.Base(filename),
		filename,
	}
}

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

func SubmitLabradarCsvFile(filename string) error {

	csvFile := buildIncomingLabradarConfig(filename)
	ctx := context.Background()

	s3Bucket, err := blob.OpenBucket(ctx, csvFile.IncomingBucket())
	if err != nil {
		return err
	}
	defer func(s3Bucket *blob.Bucket) {
		_ = s3Bucket.Close()
	}(s3Bucket)

	csvBytes, err := csvFile.ReadContents()
	if err != nil {
		return err
	}

	w, err := blob.PrefixedBucket(s3Bucket, "/incoming/").
		NewWriter(ctx, csvFile.Key, nil)
	if err != nil {
		return err
	}
	_, err = w.Write(csvBytes)
	if err != nil {
		return err
	}
	if err := w.Close(); err != nil {
		return err
	}
	return nil
}
