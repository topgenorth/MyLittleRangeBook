using System.Text;
using net.opgenorth.xero.device;
using Serilog;

namespace net.opgenorth.xero.shotview
{
    public class ShotViewXlsxParser
    {
        readonly ILogger _logger;
        readonly FileInfo _xslxFile;

        public ShotViewXlsxParser(ILogger logger, string fileName) : this(logger, new FileInfo(fileName))
        {
        }

        public ShotViewXlsxParser(ILogger logger, FileInfo xslxFile)
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
                Notes = GetNotes(ws).ToString().TrimEnd()
            };

            return s;
        }

        static StringBuilder GetNotes(IXLWorksheet ws)
        {
            const string PERIOD = ". ";
            const string BULLET = "    * ";

            StringBuilder notes = new StringBuilder(BULLET);
            notes.Append("Sheet title: ");
            notes.Append(ws.Name);
            notes.AppendLine(PERIOD);

            notes.Append(BULLET);
            notes.Append("Session title: ");
            notes.Append(ws.Cell(1, 1).GetText());
            notes.AppendLine(PERIOD);

            return notes;
        }

        DateTime GetSessionDate(IXLWorksheet ws)
        {
            const string X = "DATE";

            var row = ws.FindRowThatStartsWith(X);
            var d = row?.GetDateTimeUTC();

            if (d is null)
            {
                return DateTime.UtcNow;
            }
            else
            {
                return d.Value;
            }

        }
    }
}
