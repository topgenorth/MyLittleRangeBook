using CommunityToolkit.HighPerformance;
using Dynastream.Fit;
using net.opgenorth.xero.device;

namespace net.opgenorth.xero.FitFile
{
    public class SimpleFitReader
    {
        readonly ILogger _logger;
        readonly XeroParser _xeroParser;
        public SimpleFitReader(ILogger logger)
        {
            _logger = logger.ForContext<SimpleFitReader>();
            _xeroParser = new XeroParser(logger);
        }

        public async Task<int> Read(string filename, CancellationToken ct)
        {
            _logger.Information("Processing {FitFile}...", filename);
            var fitData = await LoadFile(filename, ct);

            await using var stream = fitData.AsStream();
            var shotSession = _xeroParser.Decode(stream);
            shotSession.FileName = filename;
            
            _logger.Information("{ShotSession}", shotSession);
            _logger.Information("Finished with {FitFile}.", filename);
            return 0;
        }
        

        async Task<ReadOnlyMemory<byte>> LoadFile(string filename, CancellationToken token)
        {
            await using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            var result = new byte[fs.Length];
            var bytesRead = await fs.ReadAsync(result, 0, (int)fs.Length, token).ConfigureAwait(false);
            _logger.Verbose("Loaded {BytesRead} bytes from {Filename}", bytesRead, filename);

            return result;
        }
    }
}
