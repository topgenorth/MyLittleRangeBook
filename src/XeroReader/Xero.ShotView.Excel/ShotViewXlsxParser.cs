using System.Text;
using net.opgenorth.xero.device;
using Serilog;

namespace net.opgenorth.xero.shotview
{
    public class ShotViewXlsxParser : IDisposable
    {
        readonly ILogger _logger;
        readonly FileInfo _xslxFile;
        IXLWorkbook _workbook;
        IXLWorksheet _worksheet;

        public ShotViewXlsxParser(ILogger logger, string fileName) : this(logger, new FileInfo(fileName))
        {
        }

        public ShotViewXlsxParser(ILogger logger, FileInfo xslxFile)
        {
            _logger = logger;
            _xslxFile = xslxFile;
        }

        public void Dispose() => _workbook?.Dispose();

        public override string ToString() => _xslxFile.FullName;

        public ShotSession GetShotSession(int sheetNumber)
        {
            _workbook = new XLWorkbook(_xslxFile.FullName);
            _worksheet = _workbook.Worksheets.ElementAt(sheetNumber);

            ShotSession s = new() { FileName = _xslxFile.Name };

            List<Action<ShotSession>>? mutators = new()
            {
                GetDateFromWorksheet,
                CreateNotesFromWorksheet,
                GetProjectileWeightFromWorksheet,
                GetShotsFromWorksheet
            };

            s.Mutate(mutators);

            return s;
        }

        void GetProjectileWeightFromWorksheet(ShotSession s)
        {
            IXLRow? row = _worksheet.FindRowThatStartsWith("Projectile Weight (GRAINS)");
            s.ProjectileWeight = row?.GetInteger() ?? 0;
        }

        void GetDateFromWorksheet(ShotSession s)
        {
            IXLRow? row = _worksheet.FindRowThatStartsWith("DATE");
            DateTime? d = row?.GetDateTimeUTC();
            s.SessionTimestamp = d ?? DateTime.UtcNow.ToUniversalTime();
        }

        void CreateNotesFromWorksheet(ShotSession s)
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

        void GetShotsFromWorksheet(ShotSession s)
        {
            IXLRow? shotDelimiter = _worksheet.FindRowThatStartsWith("-");
            int upperLimit = shotDelimiter.RowNumber();
            IXLRows? shotRows = _worksheet.Rows(3, upperLimit);

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

                shot.Speed = new ShotSpeed(speed.Value, "fps");

                s.AddShot(shot);
            }
        }
    }
}
