package main

import (
	"flag"
	"fmt"
	"log"
	"os"

	"github.com/muktihari/fit/decoder"
	"github.com/muktihari/fit/profile/untyped/mesgnum"
	"github.com/muktihari/fit/proto"
)

type recordListener struct{}

func (rl *recordListener) OnMesg(m proto.Message) {
	if m.Num != mesgnum.Record {
		return
	}

	// This SDK exposes decoded messages through the listener; the exact
	// record field access depends on the message type produced by the SDK.
	// If your FIT file contains speed in m/s, convert to ft/s here:
	// ft/s = m/s * 3.28084
	//
	// Add the message-specific field extraction once the record type is known.
	fmt.Println("Record message found")
}

func main() {
	filename := flag.String("file", "", "Path to FIT file")
	flag.Parse()

	if *filename == "" {
		log.Fatal("Provide FIT file with -file flag")
	}

	f, err := os.Open(*filename)
	if err != nil {
		log.Fatal(err)
	}
	defer f.Close()

	var errDecode error

	dec := decoder.New(
		f,
		decoder.WithMesgListener(new(recordListener)),
	)

	for dec.Next() {
		if _, err := dec.Decode(); err != nil {
			errDecode = err
			break
		}
	}

	if errDecode != nil {
		log.Fatal(errDecode)
	}
}
