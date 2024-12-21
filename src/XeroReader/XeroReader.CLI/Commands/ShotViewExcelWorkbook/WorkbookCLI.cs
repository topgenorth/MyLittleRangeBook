using System.ComponentModel.DataAnnotations;
using ConsoleAppFramework;
using Microsoft.Extensions.Options;
using net.opgenorth.xero.data.sqlite;
using net.opgenorth.xero.shotview;

namespace net.opgenorth.xero.Commands.ShotViewExcelWorkbook
{
    // ReSharper disable once InconsistentNaming
    public class WorkbookCLI
    {
        static readonly Task<int> s_failure = Task.FromResult(1);
        static readonly Task<int> s_success = Task.FromResult(0);
        readonly ILogger _logger;
        readonly IOptionsSnapshot<GarminShotViewSqliteOptions> _options;

        FileInfo? _file;

        public WorkbookCLI(ILogger logger, IOptionsSnapshot<GarminShotViewSqliteOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        /// <summary>
        ///     Import a session to the console from the numbered worksheet contained in the ShotView export workbook.
        /// </summary>
        /// <param name="filename">The name of the Excel workbook.</param>
        /// <param name="sheetNumber">The number of the sheet to read. Zero-indexed.</param>
        /// <returns></returns>
        [Command("import")]
        public Task<int> ImportSheet(string filename, [Range(0, 100)] int sheetNumber = 0)
        {
            _file = new FileInfo(filename);

            try
            {
                WorkbookSession? session = ReadShotSession(sheetNumber);
                if (session == null)
                {
                    _logger.Warning($"Could not read from the sheet #{sheetNumber} from {filename}.");
                }
                else
                {
                    MyLittleRangeBookDb db = new(_logger, _options);
                    _logger.Verbose("Database at {0}", db.Filename);
                    db.UpsertSession(session);
                    _logger.Information($"Imported sheet #{sheetNumber} from {filename}.");
                }

                return s_success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error importing the worksheet");

                return s_failure;
            }
            finally
            {
                _file = null;
            }
        }


        /// <summary>
        ///     Display a session to the console from the numbered worksheet contained in the ShotView export workbook.
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

                return s_success;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Could not read the file {ExcelFile}", filename);

                return s_failure;
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

            XslxWorksheetAdapter? x = new(_logger, _file.FullName);
            WorkbookSession session = x.GetShotSession(sheetNumber);
            _logger.Information("Read the file {ExcelFile}", _file.FullName);

            return session;
        }
    }
}
