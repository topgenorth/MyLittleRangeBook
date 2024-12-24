using System.Text;
using net.opgenorth.xero.device;
using Serilog;

namespace net.opgenorth.xero.shotview
{
    public class XslxWorksheetAdapter : IDisposable
    {
        readonly ILogger _logger;
        readonly FileInfo _xslxFile;
        IXLWorkbook _workbook;
        IXLWorksheet _worksheet;

        public XslxWorksheetAdapter(ILogger logger, string fileName) : this(logger, new FileInfo(fileName))
        {
        }

        public XslxWorksheetAdapter(ILogger logger, FileInfo xslxFile)
        {
            _logger = logger;
            _xslxFile = xslxFile;
        }

        public void Dispose() => _workbook?.Dispose();

        public override string ToString() => _xslxFile.FullName;

        public void WriteMetadataToWorksheet(WorkbookSession session)
        {
            // const string x = "All shots included in the calculations";
            // var lastRow = _worksheet.FindRowThatStartsWith(x);
            // if (lastRow is null)
            // {
            //     _logger.Warning("Could not find the last row in {sheetName}", _worksheet.Name);
            //     return;
            // }
            //
            // var i = lastRow.RowNumber() + 1;
            // [TO20241222] Just write to G1.
            _worksheet.Cell(1, "G").Value = session.Id;
            _workbook.Save();
        }

        public WorkbookSession GetShotSession(int sheetNumber)
        {
            _workbook = new XLWorkbook(_xslxFile.FullName);
            _worksheet = _workbook.Worksheets.ElementAt(sheetNumber);
            string? id = _worksheet.Cell(1, "G").Value.GetText();
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            WorkbookSession s = new(id) { FileName = _xslxFile.Name, SheetNumber = sheetNumber};

            List<Action<WorkbookSession>> mutators =
            [
                GetSessionDateFromWorksheet,
                CreateNotesFromWorksheet,
                GetProjectileWeightFromWorksheet,
                GetShotsFromWorksheet,
                GetSheetName
            ];

            s.Mutate(mutators);

            _logger.Verbose("Loaded session from {sheetName}", s.SheetName);
            return s;
        }


        void GetSheetName(WorkbookSession s)
        {
            string sheetName = $"{_xslxFile.Name}[{s.SheetNumber}]-{_worksheet.Name}";
            s.SheetName = sheetName;
        }

        void GetProjectileWeightFromWorksheet(WorkbookSession s)
        {
            IXLRow? row = _worksheet.FindRowThatStartsWith("Projectile Weight (GRAINS)");
            s.ProjectileWeight = row?.GetInteger() ?? 0;
        }

        void GetSessionDateFromWorksheet(WorkbookSession s)
        {
            IXLRow? row = _worksheet.FindRowThatStartsWith("DATE");
            DateTime? d = row?.GetDateTimeUTC();
            s.DateTimeUtc = d ?? DateTime.UtcNow;
        }

        void CreateNotesFromWorksheet(WorkbookSession s)
        {
            const string period = ". ";
            const string bullet = "    * ";

            StringBuilder notes = new(bullet);
            notes.Append("Sheet title: ");
            notes.Append(_worksheet.Name);
            notes.AppendLine(period);

            notes.Append(bullet);
            notes.Append("Session title: ");
            notes.Append(_worksheet.Cell(1, 1).GetText());
            notes.AppendLine(period);

            s.Notes = notes.ToString().TrimEnd();
        }

        void GetShotsFromWorksheet(WorkbookSession s)
        {
            IXLRow? shotDelimiter = _worksheet.FindRowThatStartsWith("-");
            int upperLimit = shotDelimiter.RowNumber();
            IXLRows? shotRows = _worksheet.Rows(3, upperLimit);

            foreach (IXLRow row in shotRows)
            {
                Shot? shot = new()
                {
                    CleanBore = row.GetBool("G"),
                    ColdBore = row.GetBool("H"),
                    Notes = row.GetString("I")
                };

                int? shotNumber = row.GetInteger("A");
                if (shotNumber is null)
                {
                    continue;
                }
                shot.ShotNumber = shotNumber.Value;

                int? speed = row.GetInteger();
                if (speed is null)
                {
                    continue;
                }

                // TODO [TO20241222] Assumption is that we're FPS.
                shot.Speed = new ShotSpeed(speed.Value, "fps");

                shot.DateTimeUtc = ConvertShotTimeToShotDateTimeUtc(s.DateTimeUtc, row.GetString("F"));

                s.AddShot(shot);
            }
        }

        DateTime ConvertShotTimeToShotDateTimeUtc(DateTime sessionDateUtc, string timeText)
        {
            if (string.IsNullOrWhiteSpace(timeText))
            {
                return sessionDateUtc;
            }

            if (!TimeOnly.TryParse(timeText, out var shotTime))
            {
                return sessionDateUtc;
            }

            var localDt = sessionDateUtc.ToLocalTime();
            var shotDate = new DateTime(localDt.Year, localDt.Month, localDt.Day,
                shotTime.Hour, shotTime.Minute, shotTime.Second);
            shotDate = DateTime.SpecifyKind(shotDate, DateTimeKind.Local);

            return shotDate.ToUniversalTime();

        }
    }
}

