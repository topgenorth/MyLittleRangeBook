using System.ComponentModel.DataAnnotations;
using ConsoleAppFramework;
using net.opgenorth.xero.shotview;

namespace net.opgenorth.xero.Commands.ShotViewExcelWorkbook
{
    // ReSharper disable once InconsistentNaming
    public class WorkbookCLI
    {
        readonly Task<int> _failure = Task.FromResult(1);
        readonly ILogger _logger;
        readonly Task<int> _success = Task.FromResult(0);
        FileInfo? _file;
        public WorkbookCLI(ILogger logger) => _logger = logger;

        /// <summary>
        ///     Import a session from the numbered worksheet contained in the ShotView export workbook.
        /// </summary>
        /// <param name="filename">The name of the Excel workbook.</param>
        /// <param name="sheetNumber">The number of the sheet to read. Zero-indexed.</param>
        /// <returns></returns>
        [Command("view")]
        public Task<int> DisplayWorksheetInConsole(string filename, [Range(0, 100)] int sheetNumber = 0)
        {
            _file = new FileInfo(filename);
            try
            {
                WorkbookSession? session = ReadShotSession(sheetNumber);
                WriteToConsole(session);

                return _success;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Could not read the file {ExcelFile}", filename);

                return _failure;
            }
            finally
            {
                _file = null;
            }
        }

        void WriteToConsole(WorkbookSession? session)
        {
            if (session is null)
            {
                return;
            }

            ShotViewExcelSpreadsheetTemplate? page = new(session);
            string? content = page.TransformText();
            Console.Write(content);
        }

        WorkbookSession? ReadShotSession(int sheetNumber)
        {
            if (_file is null)
            {
                _logger.Verbose("No file specified.");

                return null;
            }

            ShotViewXlsxParser? x = new(_logger, _file.FullName);
            WorkbookSession session = x.GetShotSession(sheetNumber);
            WorkbookSession ws = new(session) { SheetNumber = sheetNumber };
            _logger.Information("Read the file {ExcelFile}", _file.FullName);

            return ws;
        }
    }
}
