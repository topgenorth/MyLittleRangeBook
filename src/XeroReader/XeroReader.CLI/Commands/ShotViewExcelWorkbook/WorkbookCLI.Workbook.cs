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
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("import")]
        public async Task<int> ImportWorkbook(string filename, CancellationToken ct)
        {
            _file = new FileInfo(filename);
            _logger.Verbose("Database at {0}", _repo.Filename);

            if (ct.IsCancellationRequested)
            {
                return 0;
            }

            try
            {
                using XlsxAdapter? xlsx = new(_logger, _file.FullName);
                foreach (WorkbookSession? s in xlsx.GetAllSessions())
                {
                    try
                    {
                        await _repo.UpsertSession(s, ct);
                        _logger.Verbose("Imported {name}", s.SheetName);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Whoops");
                    }
                }

                return 0;
            }
            catch (ArgumentException aex)
            {
                _logger.Information("Please make sure that this is an Excel 2007 file with an XLSX extension.?");

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
