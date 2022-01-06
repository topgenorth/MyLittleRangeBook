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
type labradarS3FileSubmission struct {
	*aws.Config
	BucketName string
	Key        string
	SourceFile string
	Bytes      []byte
	err        error
	ctx        context.Context
}

func (f *labradarS3FileSubmission) LoadFile() {
	var err error
	f.Bytes, err = ioutil.ReadFile(f.SourceFile)
	if err != nil {
		f.err = fmt.Errorf("could not read the file %s: %w", f.SourceFile, err)
		f.Bytes = make([]byte, 0)
	}
}

func (f *labradarS3FileSubmission) SubmitToS3() {

	if f.ctx == nil {
		f.err = fmt.Errorf("must set the context before trying to connect to Amazon S3")
		return
	}

	if f.err != nil {
		return
	}

	if f.Bytes == nil || len(f.Bytes) < 1 {
		f.err = fmt.Errorf("must load the file %s before trying to submit it", f.SourceFile)
		return
	}

	bucket, err := blob.OpenBucket(f.ctx, "s3://"+f.BucketName)
	if err != nil {
		f.err = fmt.Errorf("could not open the S3 bucket %s: %w", f.BucketName, err)
		return
	}

	writer, err := blob.PrefixedBucket(bucket, "incoming").NewWriter(f.ctx, "/"+f.Key, nil)
	if err != nil {
		f.err = fmt.Errorf("could not open a writer to the S3 bucket %s: %w", f.BucketName, err)
		return
	}

	_, err = writer.Write(f.Bytes)
	if err != nil {
		f.err = fmt.Errorf("could not write the contents of the file %s to the S3 bucket %s: %w", f.SourceFile, f.BucketName, err)
		return
	}
	defer func(b *blob.Bucket, w *blob.Writer) {
		_ = w.Close()
		_ = b.Close()

		//if err := w.Close(); err != nil {
		//	f.err = fmt.Errorf("could not close the Writer to the S3 bucket %s: %w", f.BucketName, err)
		//}
	}(bucket, writer)

}

func newLabradarS3File(filename string) *labradarS3FileSubmission {
	csv := &labradarS3FileSubmission{
		Config:     buildAwsConfig(),
		BucketName: S3_LABRADAR_CSV_BUCKET_FOR_INCOMING_FILES,
		Key:        filepath.Base(filename),
		SourceFile: filename,
		Bytes:      nil,
		err:        nil,
		ctx:        context.Background(),
	}

	return csv
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

	csvFile := newLabradarS3File(filename)
	csvFile.LoadFile()
	csvFile.SubmitToS3()

	if csvFile.err != nil {
		return fmt.Errorf("Could not submit the Labradar file: %w", csvFile.err)
	}
	return csvFile.err
}
