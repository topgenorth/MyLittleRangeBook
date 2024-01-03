package exportfile

import (
	"bytes"
	"io"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

type csvFileContentsHelper func(t *testing.T) io.Reader

// series14Contents is a copy of a CSV file that was exported using the Labradar app (on Android). It is a text file
// which appears to have been cleaned up.
var series14Contents = `sep=;
Device ID;LBR0013797202206060629;;

Series No;#014;;
Total number of shots;0010;;

Units velocity;fps;;
Units distances;m;;
Units kinetic energy;j;;
Units weight;grain;;

Stats - Average;2874.90;fps;
Stats - Highest;3007.98;fps;
Stats - Lowest;2705.99;fps;
Stats - Ext. Spread;301.99;fps;
Stats - Std. Dev;102.49;fps;

Shot ID;V0;V10.00;V20.00;V30.00;V40.00;V50.00;Ke0;Ke10.00;Ke20.00;Ke30.00;Ke40.00;Ke50.00;PF10.00;Proj. Weight;Date;Time
0001;2705.99;2661.92;2617.12;2572.16;2527.93;2495.31;3967.27;3839.11;3710.98;3584.58;3462.34;3373.56;479.16;180.00;2023-11-19;19:56:41;
0002;2774.01;2729.25;2683.34;2637.68;2594.11;2553.52;4169.24;4035.78;3901.14;3769.50;3646.00;3532.79;491.22;180.00;2023-11-19;19:56:58;
0003;2814.94;2768.99;2722.29;2675.98;2630.19;2587.19;4293.16;4154.15;4015.22;3879.78;3748.13;3626.57;498.42;180.00;2023-11-19;19:57:29;
0004;2800.14;2754.95;2708.75;2663.68;2617.89;2573.84;4248.15;4112.14;3975.37;3844.20;3713.16;3589.24;495.90;180.00;2023-11-19;20:00:28;
0005;2839.92;2792.55;2744.09;2695.73;2647.64;2616.04;4369.71;4225.15;4079.79;3937.25;3798.02;3707.90;502.74;180.00;2023-11-19;20:01:15;
0006;2905.01;2858.48;2811.21;2763.56;2717.89;2671.16;4572.31;4427.01;4281.82;4137.90;4002.23;3865.81;514.44;180.00;2023-11-19;20:01:40;
0007;2944.92;2897.68;2849.29;2801.45;2753.12;2708.96;4698.79;4549.26;4398.59;4252.12;4106.67;3975.98;521.64;180.00;2023-11-19;20:02:14;
0008;2960.29;2913.07;2864.69;2816.84;2768.98;2723.17;4747.99;4597.70;4446.27;4298.98;4154.14;4017.81;524.34;180.00;2023-11-19;20:02:44;
0009;2995.78;2946.75;2896.74;2847.32;2797.12;2747.84;4862.50;4704.64;4546.30;4392.52;4238.98;4090.93;530.46;180.00;2023-11-19;20:04:48;
0010;3007.98;2961.05;2913.18;2864.62;2816.79;2769.82;4902.19;4750.42;4598.07;4446.04;4298.83;4156.64;532.98;180.00;2023-11-19;20:05:17;
`

func getRelativePathForTestFile(t *testing.T, file string) string {
	// Get the current file's path
	_, filename, _, ok := runtime.Caller(0)
	if !ok {
		t.Fatalf("Failed to get current file's path")
	}

	// Get the directory of the current file
	dir := filepath.Dir(filename)

	// Assume the file you want to read is in the same directory as the test file
	filePath := filepath.Join(dir, file)
	return filePath
}
func series11(t *testing.T) io.Reader {
	filePath := getRelativePathForTestFile(t, "sr0011_report.csv")
	file, err := os.Open(filePath)
	if err != nil {
		t.Fatalf("could not open the file %s", filePath)
	}
	defer func(file *os.File) {
		_ = file.Close()
	}(file)

	// Read the file contents into a byte slice
	fileInfo, err := file.Stat()
	if err != nil {
		t.Fatalf("can't determine the size of %s", filePath)
	}
	fileSize := fileInfo.Size()
	data := make([]byte, fileSize)

	_, err = file.Read(data)
	if err != nil && err != io.EOF {
		t.Fatalf("there was a problem reading the contents of %s", filePath)
	}

	// Create an io.Reader from the byte slice
	reader := bytes.NewReader(data)
	return reader

}
func series14(t *testing.T) io.Reader {
	return strings.NewReader(series14Contents)
}
