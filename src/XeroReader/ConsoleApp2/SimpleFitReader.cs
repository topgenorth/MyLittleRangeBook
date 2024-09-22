using Dynastream.Fit;

namespace ConsoleApp2;

public class SimpleFitReader
{
    readonly ILogger _logger;
    public SimpleFitReader(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<int> Read(string filename)
    {
        _logger.Information("Processing file {filename}", filename);

        var fitListener = new FitListener();
        var decodeDemo = new Decode();
        decodeDemo.MesgEvent += fitListener.OnMesg;

        _logger.Information("Decoding...");
        await using var fitSource = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        decodeDemo.Read(fitSource);

        var fitMessages = fitListener.FitMessages;
        foreach (var msg in fitMessages.DeviceInfoMesgs)
        {
            _logger.Information(
                $"Timestamp: {msg?.GetTimestamp().ToString()}, SoftwareVersion: {msg?.GetSoftwareVersion()}");
        }

        foreach (var msg in fitListener.FitMessages.FileIdMesgs)
        {
            _logger.Information(
                $"File ID Timestamp: {msg?.GetManufacturer()}, Serial #: {msg?.GetGarminProduct()} {msg?.GetSerialNumber()}");
        }

        foreach (var msg in fitMessages.ChronoShotSessionMesgs)
        {
            var t = msg?.GetTimestamp();
            _logger.Information(
                $"Session Timestamp: {msg?.GetTimestamp().ToString()}, Shot Count: {msg?.GetShotCount()}");
        }

        _logger.Information("Done");
        return 0;
    }
}
