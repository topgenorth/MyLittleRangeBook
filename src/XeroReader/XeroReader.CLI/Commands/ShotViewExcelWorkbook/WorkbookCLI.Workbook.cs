using ConsoleAppFramework;
using net.opgenorth.xero.shotview;

namespace net.opgenorth.xero.Commands.ShotViewExcelWorkbook
{
    public partial class WorkbookCLI
    {
        /// <summary>
        ///     Import all of the worksheets in the workbook.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        [Command("import")]
        public async Task<int> ImportWorkbook(string filename, CancellationToken ct)
        {
            _file = new FileInfo(filename);
            try
            {
                _logger.Information("{appName}", WorksheetExtensions.GetAppNameAndVersion());
                using XlsxAdapter? xlsx = new(_logger, _file.FullName);
                foreach (WorkbookSession? s in xlsx.GetAllSessions())
                {
                    await _repo.UpsertSession(s);
                    _logger.Verbose("Imported {name}", s.SheetName);
                }

                return 0;
            }
            catch (ArgumentException aex)
            {
                _logger.Information(aex, "Is this a 2007  Excel file?");

                return 1;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Could not import the XSLX {filename}", filename);

                return 1;
            }
        }
    }
}
