using Dynastream.Fit;
using Serilog;

namespace ConsoleApp2;

public class SimpleFitReader
{
    public async Task<int> Read(string filename)
    {
        var log = Serilog.Log.Logger.ForContext<SimpleFitReader>(); 
        // filename = "C:\\Users\\tom.opgenorth\\Dropbox\\Firearms\\Shot_Sessions\\09-08-2024_18-04-40.fit";

        log.Information("Processing file {filename}", filename);

        var fitListener = new FitListener();
        var decodeDemo = new Decode();
        decodeDemo.MesgEvent += fitListener.OnMesg;

        log.Information("Decoding...");
        await using var fitSource = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        decodeDemo.Read(fitSource);

        var fitMessages = fitListener.FitMessages;
        foreach (var msg in fitMessages.DeviceInfoMesgs)
        {
            log.Information($"Timestamp: {msg?.GetTimestamp().ToString()}, SoftwareVersion: {msg?.GetSoftwareVersion()}");
        }

        foreach (var msg in fitListener.FitMessages.FileIdMesgs)
        {
            log.Information(
                $"File ID Timestamp: {msg?.GetManufacturer()}, Serial #: {msg?.GetGarminProduct()} {msg?.GetSerialNumber()}");
            ;
        }

        foreach (var msg in fitMessages.ChronoShotSessionMesgs)
        {
            var t = msg?.GetTimestamp();
            log.Information($"Session Timestamp: {msg?.GetTimestamp().ToString()}, Shot Count: {msg?.GetShotCount()}");
        }

        log.Information("Done");
        await Log.CloseAndFlushAsync();

        return 0;
    }
}
