package main

import (
	"fit-reader/deviceinfo"
	"fit-reader/shotsession"
	"flag"
	"fmt"
	"log"
	"os"

	"github.com/muktihari/fit/decoder"
	"github.com/muktihari/fit/proto"
)

type recordListener struct{}

func debugMessage(m proto.Message) {

	fmt.Fprintf(os.Stdout, "  > Unknown proto.Message - Num: %s\n", m.Num)

	if len(m.Fields) > 0 {
		fmt.Fprintf(os.Stdout, "      Fields (%d)\n", len(m.Fields))
		for i, f := range m.Fields {
			fmt.Fprintf(os.Stdout, "        ")
			fmt.Fprintf(os.Stdout, "%d: %s (%d)\n", i, f.Name, f.Num)
		}
	} else {
		fmt.Fprintf(os.Stdout, "No fields available.\n")
	}

}

func (rl *recordListener) OnMesg(m proto.Message) {

	serialNum := deviceinfo.ParseSerialNumber(m)
	if serialNum > 0 {
		fmt.Fprintf(os.Stdout, "Serial Number: %d\n", serialNum)
		return
	}

	session := shotsession.ParseShotSession(m)
	if session != nil {
		fmt.Fprintf(os.Stdout, "Shot Session\n")
		return
	}

	if shotsession.ParseShotSession(m) == nil {
		debugMessage(m)
	}
}

func printHeader(filename string) {
	fmt.Println("===== START =====")
	fmt.Println("Reading data from file ", filename)
}

func printFooter() {
	fmt.Println("===== FINISHED =====")
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
	defer func(f *os.File) {
		_ = f.Close()
	}(f)

	printHeader(*filename)
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

	printFooter()
}
