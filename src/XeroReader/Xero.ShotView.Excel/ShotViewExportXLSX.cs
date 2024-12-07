// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Text;
using net.opgenorth.xero.device;
using Serilog;

namespace net.opgenorth.xero.shotview
{

    internal  static class WorksheetExtensions
    {
        public static string GetString(this IXLWorksheet ws, int row, int col)
        {
            return ws.Cell(row, col).GetText() ?? string.Empty;
        }
    }
    public class ShotViewExportXLSX
    {
        readonly ILogger _logger;

        public ShotViewExportXLSX(ILogger logger) => _logger = logger;

        public void ReadFile(string fileName)
        {
            var s = WorkbookToShotSession(new FileInfo(fileName), 1);
            _logger.Information($"{s}");
        }

        internal ShotSession WorkbookToShotSession(FileInfo file, int sheetNumber)
        {
            using var wb = new XLWorkbook(file.FullName);
            var ws = wb.Worksheets.ToList()[sheetNumber];
            var s = new ShotSession { FileName = file.Name };

            var notes = new StringBuilder();
            notes.Append(ws.Cell(1, 1).GetText());
            notes.Append('.');
            wb.Dispose();

            s.Notes = notes.ToString();
            return s;
        }
    }
}
