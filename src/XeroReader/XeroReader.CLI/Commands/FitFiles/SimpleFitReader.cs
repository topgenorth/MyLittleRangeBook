using CommunityToolkit.HighPerformance;
using net.opgenorth.xero.GarminFit;
using net.opgenorth.xero.Model;

namespace net.opgenorth.xero.FitFiles;

public class SimpleFitReader
{
    private readonly ILogger _logger;

    private readonly XeroParser _xeroParser;

    public SimpleFitReader(ILogger logger)
    {
        _logger = logger.ForContext<SimpleFitReader>();
        _xeroParser = new XeroParser(logger);
    }

    public async Task<int> Read(string filename, CancellationToken ct)
    {
        _logger.Information("Processing {FitFile}...", filename);
        ReadOnlyMemory<byte> fitData = await LoadFile(filename, ct);

        await using Stream stream = fitData.AsStream();
        ShotSession shotSession = _xeroParser.Decode(stream);
        shotSession.FileName = filename;

        _logger.Information("{ShotSession}", shotSession);
        _logger.Information("Finished with {FitFile}.", filename);

        return 0;
    }


    /// <summary>
    ///     Read the file from disk to ReadOnlyMemory.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task<ReadOnlyMemory<byte>> LoadFile(string filename, CancellationToken token)
    {
        await using FileStream fs = new(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] result = new byte[fs.Length];
        int bytesRead = await fs.ReadAsync(result, 0, (int)fs.Length, token).ConfigureAwait(false);
        _logger.Verbose("Loaded {BytesRead} bytes from {Filename}", bytesRead, filename);

        return result;
    }
}
