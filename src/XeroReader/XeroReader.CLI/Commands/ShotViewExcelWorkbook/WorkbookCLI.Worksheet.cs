using System.ComponentModel.DataAnnotations;
using ConsoleAppFramework;
using net.opgenorth.xero.shotview;

namespace net.opgenorth.xero.Commands.ShotViewExcelWorkbook
{
    // ReSharper disable once InconsistentNaming
    public partial class WorkbookCLI
    {

        /// <summary>
        ///     Import a session to the console from the numbered worksheet contained in the ShotView export workbook.
        /// </summary>
        /// <param name="filename">The name of the Excel workbook.</param>
        /// <param name="sheetNumber">The number of the sheet to read. Zero-indexed.</param>
        /// <returns></returns>
        [Command("import sheet")]
        public async Task<int> ImportSheet(string filename, [Range(0, 100)] int sheetNumber, CancellationToken ct)
        {
            _file = new FileInfo(filename);
            _logger.Verbose("Database at {0}", _repo.Filename);

            try
            {
                using IShotViewXslxAdapter xlsx = new XlsxAdapter(_logger, _file.FullName);

                WorkbookSession? session = xlsx.GetShotSession(sheetNumber);
                if (session == null)
                {
                    _logger.Warning($"Could not read from the sheet #{sheetNumber} from {filename}.");
                }
                else
                {
                    await _repo.UpsertSession(session, ct);
                    _logger.Information($"Imported sheet #{sheetNumber} from {filename}.");
                }

                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error importing the worksheet");

                return 0;
            }
            finally
            {
                _file = null;
            }
        }

    }
}
