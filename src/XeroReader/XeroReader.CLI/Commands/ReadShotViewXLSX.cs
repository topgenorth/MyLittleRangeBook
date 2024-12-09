using net.opgenorth.xero.shotview;

namespace net.opgenorth.xero
{
    public class ReadShotViewXLSX
    {
        readonly ILogger _logger;

        public ReadShotViewXLSX(ILogger logger) => _logger = logger;

        /// <summary>
        /// Read the shot data from the specified worksheet within the named Excel spreadsheet.
        /// </summary>
        /// <param name="filename">The name of the Excel workbook.</param>
        /// <param name="sheetNumber">The number of the sheet to read. Zero-indexed.</param>
        /// <returns></returns>
        public Task<int> ReadWorksheet(string filename, int sheetNumber)
        {
            try
            {
                var x = new ShotViewXlsxParser(_logger, filename);
                var session = x.GetShotSession(sheetNumber);

                _logger.Information("Read the file {ExcelFile}", filename);

                var page = new ShotSessionTemplate(session);
                var content = page.TransformText();

                Console.Write(content);
                return Task.FromResult(0);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Could not read the file {ExcelFile}", filename);
                return Task.FromResult(1);
            }
        }
    }
}
