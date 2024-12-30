using System.Text;
using net.opgenorth.xero.device;
using Serilog;

namespace net.opgenorth.xero.shotview
{
    public class XlsxAdapter : IShotViewXslxAdapter
    {
        /// <summary>
        ///     Arbitrary value.
        /// </summary>
        public const int MaxiumumNumberOfSheets = 100;

        readonly FileInfo _file;
        readonly ILogger _logger;
        IXLWorkbook? _workbook;
        IXLWorksheet? _worksheet;

        public XlsxAdapter(ILogger logger, string fileName) : this(logger, new FileInfo(fileName))
        {
        }

        public XlsxAdapter(ILogger logger, FileInfo file)
        {
            _logger = logger;
            _file = file;
        }

        public void Dispose() => _workbook?.Dispose();

        public string Filename => _file.FullName;
        public override string ToString() => _file.FullName;


        public WorkbookSession? GetShotSession(int sheetNumber)
        {
            _workbook = new XLWorkbook(_file.FullName);

            try
            {
                _worksheet = _workbook.Worksheets.ElementAt(sheetNumber);
            }
            catch (ArgumentOutOfRangeException)
            {
                // [TO20241227] No sheet - no session.
                return null;
            }

            WorkbookSession s = new()
            {
                FileName = _file.Name,
                SheetNumber = sheetNumber,
                SheetName = $"{_file.Name}[{sheetNumber}]"
            };

            List<Action<WorkbookSession>> mutators =
            [
                GetSessionDateFromWorksheet,
                CreateNotesFromWorksheet,
                GetProjectileWeightFromWorksheet,
                GetShotsFromWorksheet
            ];

            s.Mutate(mutators);

            _logger.Verbose("Loaded session from {sheetName}", s.SheetName);

            return s;
        }

        public IEnumerable<WorkbookSession> GetAllSessions()
        {

            for (int i = 0; i < MaxiumumNumberOfSheets; i++)
            {
                WorkbookSession? session = GetShotSession(i);
                if (session is null)
                {
                    break;
                }

                yield return session;
            }

        }

        void GetProjectileWeightFromWorksheet(WorkbookSession s)
        {
            // TODO [TO20241228] We can get the weight units from this cell too.
            IXLRow? row = _worksheet!.FindRowThatStartsWith("Projectile Weight");
            s.ProjectileWeight = row?.GetInteger() ?? 0;
        }

        void GetSessionDateFromWorksheet(WorkbookSession s)
        {
            IXLRow? row = _worksheet!.FindRowThatStartsWith("DATE");
            DateTime? d = row?.GetDateTimeUTC();
            s.DateTimeUtc = d ?? DateTime.UtcNow;
        }

        void CreateNotesFromWorksheet(WorkbookSession s)
        {
            const string period = ". ";
            const string bullet = "    * ";

            StringBuilder notes = new(bullet);
            notes.Append("Sheet title: ");
            notes.Append(_worksheet!.Name);
            notes.AppendLine(period);

            notes.Append(bullet);
            notes.Append("Session title: ");
            notes.Append(_worksheet!.Cell(1, 1).GetText());
            notes.AppendLine(period);

            s.Notes = notes.ToString().TrimEnd();
        }

        void GetShotsFromWorksheet(WorkbookSession s)
        {
            IXLRow? shotDelimiter = _worksheet!.FindRowThatStartsWith("-");
            int upperLimit = shotDelimiter!.RowNumber();
            IXLRows? shotRows = _worksheet!.Rows(3, upperLimit);

            foreach (IXLRow row in shotRows)
            {
                Shot? shot = new()
                {
                    CleanBore = row.GetBool("G"), ColdBore = row.GetBool("H"), Notes = row.GetString("I")
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

            if (!TimeOnly.TryParse(timeText, out TimeOnly shotTime))
            {
                return sessionDateUtc;
            }

            DateTime localDt = sessionDateUtc.ToLocalTime();
            DateTime shotDate = new(localDt.Year, localDt.Month, localDt.Day,
                shotTime.Hour, shotTime.Minute, shotTime.Second);
            shotDate = DateTime.SpecifyKind(shotDate, DateTimeKind.Local);

            return shotDate.ToUniversalTime();
        }
    }
}
