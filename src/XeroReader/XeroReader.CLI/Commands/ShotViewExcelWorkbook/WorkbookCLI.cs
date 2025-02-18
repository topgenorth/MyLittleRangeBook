using System.ComponentModel.DataAnnotations;
using ConsoleAppFramework;
using Microsoft.Extensions.Options;
using net.opgenorth.xero.data.sqlite;
using net.opgenorth.xero.shotview;

namespace net.opgenorth.xero.ShotViewExcelWorkbook;

/// <summary>
///     Entrypoint for all the commands.
/// </summary>
// ReSharper disable once InconsistentNaming
public class WorkbookCLI
{
    private static readonly Task<int> s_failure = Task.FromResult(1);
    private static readonly Task<int> s_success = Task.FromResult(0);
    private readonly ILogger _logger;
    private readonly IOptionsSnapshot<SqliteOptions> _options;
    private readonly MyLittleRangeBookRepository _repo;
    private FileInfo? _file;

    public WorkbookCLI(ILogger logger, IOptionsSnapshot<SqliteOptions> options, MyLittleRangeBookRepository repo)
    {
        _logger = logger;
        _options = options;
        _repo = repo;
    }


    /// <summary>
    ///     Takes the specified file, and converts it to a markdown.
    /// </summary>
    /// <param name="filename">The ShotView file for import.</param>
    /// <param name="destination">The morkdown file for export.</param>
    /// <param name="sheetNumber">The number of the sheet to covert. Zero indexed. Defaults to the first sheet.</param>
    /// <returns></returns>
    [Command("export markdown")]
    public Task<int> ExportToMarkdown(string filename, string destination, [Range(0, 100)] int sheetNumber = 0)
    {
        _file = new FileInfo(filename);
        try
        {
            using XlsxAdapter? xlsx = new(_logger, _file.FullName);
            WorkbookSession? session = xlsx.GetShotSession(sheetNumber);

            FileInfo outputFile = new(destination);
            if (outputFile.Exists)
            {
                outputFile.Delete();
            }


            WriteToMarkdown(session, outputFile);

            return s_success;
        }
        catch (Exception e)
        {
            _logger.Fatal(e, "Could not read the file {ExcelFile}", filename);

            return s_failure;
        }
        finally
        {
            _file = null;
        }
    }

    /// <summary>
    ///     Import all the worksheets in the workbook.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    [Command("import workbook")]
    public async Task<int> ImportWorkbook(string filename, CancellationToken ct)
    {
        _logger.Information("{appName}", WorksheetExtensions.GetAppNameAndVersion());

        _file = new FileInfo(filename);
        try
        {
            using XlsxAdapter? xlsx = new(_logger, _file.FullName);
            foreach (WorkbookSession? s in xlsx.GetAllSessions())
            {
                await _repo.UpsertSession(s);
                _logger.Verbose("Imported {name}", s.SheetName);
            }

            return 1;
        }
        catch (Exception e)
        {
            _logger.Fatal(e, "Could not import the XSLX {ExcelFile}", filename);

            return 0;
        }
    }

    /// <summary>
    ///     Import a session to the console from the numbered worksheet contained in the ShotView export workbook.
    /// </summary>
    /// <param name="filename">The name of the Excel workbook.</param>
    /// <param name="sheetNumber">The number of the sheet to read. Zero-indexed.</param>
    /// <returns></returns>
    [Command("import")]
    public async Task<int> ImportSheet(string filename, [Range(0, 100)] int sheetNumber = 0)
    {
        _logger.Information("{appName}", WorksheetExtensions.GetAppNameAndVersion());

        _file = new FileInfo(filename);

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
                _logger.Verbose("Database at {0}", _repo.Filename);
                await _repo.UpsertSession(session);
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
            using XlsxAdapter? xlsx = new(_logger, _file.FullName);
            WorkbookSession? session = xlsx.GetShotSession(sheetNumber);
            WriteToConsole(session);

            return s_success;
        }
        catch (Exception e)
        {
            _logger.Fatal(e, "Could not read the file {ExcelFile}", filename);

            return s_failure;
        }
        finally
        {
            _file = null;
        }
    }

    private void WriteToConsole(WorkbookSession? session)
    {
        if (session is null)
        {
            return;
        }

        ShotViewExcelSpreadsheetTemplate? page = new(session, WorksheetExtensions.GetAppNameAndVersion());
        string? content = page.TransformText();
        Console.Write(content);
    }

    private void WriteToMarkdown(WorkbookSession? session, FileInfo outputFile)
    {
        if (session is null)
        {
            return;
        }


        ShotViewExcelSpreadsheetTemplate? page = new(session, WorksheetExtensions.GetAppNameAndVersion());
        string? content = page.TransformText();

        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.Warning("Did not read any content from the file {ExcelFile}", outputFile.FullName);
            return;
        }

        using StreamWriter writer = outputFile.CreateText();
        writer.Write(content);
    }
}
