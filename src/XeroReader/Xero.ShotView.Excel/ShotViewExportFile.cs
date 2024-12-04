// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Text;
using net.opgenorth.xero.device;
using Serilog;

namespace net.opgenorth.xero.shotview
{

    class WorksheetTOShotSession
    {
        readonly ILogger _logger;

        public WorksheetTOShotSession(ILogger logger)
        {
            _logger = logger;
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
    public class ShotViewExportFile
    {
        readonly ILogger _logger;

        public ShotViewExportFile(ILogger logger) => _logger = logger;

        public void DoStuff()
        {
            const string fileName =
                "/home/tom/code/MyLittleRangeBook/src/XeroReader/data/Sessions_SEP_2024-SEP_2024.xlsx";

            var x = new WorksheetTOShotSession(_logger);
            var s = x.WorkbookToShotSession(new FileInfo(fileName), 1);

            _logger.Information($"{s}");
        }
    }
}
