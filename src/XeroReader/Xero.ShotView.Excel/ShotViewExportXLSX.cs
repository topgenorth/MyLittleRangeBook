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
                FileName = _xslxFile.Name, SessionTimestamp = GetSessionDate(ws), Notes = GetNotes(ws).ToString()
            };


            return s;
        }

        static StringBuilder GetNotes(IXLWorksheet ws)
        {
            StringBuilder notes = new();
            notes.Append("Title: ");
            notes.Append(ws.Cell(1, 1).GetText());
            notes.Append('.');

            return notes;
        }

        DateTime GetSessionDate(IXLWorksheet ws)
        {
            const string X = "DATE";
            foreach (IXLRow row in ws.Rows())
            {
                string? colA = row.Cell("A").GetText();
                if (!X.Equals(colA, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string? colB = row.Cell("B").GetText();
                bool isDate = DateTime.TryParse(colB, out DateTime dt);
                if (isDate)
                {
                    if (dt.Kind == DateTimeKind.Utc)
                    {
                        return dt;
                    }


                    return DateTime.SpecifyKind(DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime(),
                        DateTimeKind.Utc);
                }

                _logger.Verbose("Could not parse '{SessionDate}'", colB);

                break;
            }

            return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }
    }
}
