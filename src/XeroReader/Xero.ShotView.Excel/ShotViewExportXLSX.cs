using System.Text;
using net.opgenorth.xero.device;
using Serilog;

namespace net.opgenorth.xero.shotview
{
    public class ShotViewExportXLSX
    {
        readonly ILogger _logger;
        readonly FileInfo _xslxFile;

        public ShotViewExportXLSX(ILogger logger, string fileName) : this(logger, new FileInfo(fileName))
        {
        }

        public ShotViewExportXLSX(ILogger logger, FileInfo xslxFile)
        {
            _logger = logger;
            _xslxFile = xslxFile;
        }

        public override string ToString() => _xslxFile.FullName;

        public ShotSession GetShotSession(int sheetNumber)
        {
            using XLWorkbook wb = new(_xslxFile.FullName);
            IXLWorksheet ws = wb.Worksheets.ToList()[sheetNumber];
            ShotSession s = new()
            {
                FileName = _xslxFile.Name,
                SessionTimestamp = GetSessionDate(ws),
                Notes = GetNotes(ws).ToString().Trim()
            };


            return s;
        }

        static StringBuilder GetNotes(IXLWorksheet ws)
        {
            const string PERIOD = ". ";
            StringBuilder notes = new();

            notes.Append("Sheet title: ");
            notes.Append(ws.Name);
            notes.Append(PERIOD);

            notes.Append(" Session title: ");
            notes.Append(ws.Cell(1, 1).GetText());
            notes.Append(PERIOD);

            return notes;
        }

        DateTime GetSessionDate(IXLWorksheet ws)
        {
            const string X = "DATE";

            var row = WorksheetExtensions.FindRow(ws, X);
            return row?.GetDateTimeUTC() ?? DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }
    }
}
