using net.opgenorth.xero.shotview;

namespace net.opgenorth.xero
{
    public class CsvImporter
    {
        readonly ILogger _logger;

        public CsvImporter(ILogger logger) => _logger = logger;

        public Task<int> ImportCsv()
        {
            ShotViewExportFile? x = new(_logger);
            x.DoStuff();

            return Task.FromResult(0);
        }
    }
}
