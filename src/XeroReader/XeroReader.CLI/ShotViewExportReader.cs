using net.opgenorth.xero.shotview;

namespace net.opgenorth.xero
{
    public class ShotViewExportReader
    {
        readonly ILogger _logger;

        public ShotViewExportReader(ILogger logger) => _logger = logger;


        public Task<int> ReadXlsx(string fileName)
        {
            var x = new ShotViewExportXLSX(_logger);
            x.ReadFile(fileName);
            return Task.FromResult<int>(0);

        }
    }
}
