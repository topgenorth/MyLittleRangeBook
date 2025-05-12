
namespace net.opgenorth.xero.Excel
{
    public partial class ShotViewExcelSpreadsheetTemplate
    {
        readonly WorkbookSession _shotSession;

        public ShotViewExcelSpreadsheetTemplate(WorkbookSession shotSession, string appVersion)
        {
            _shotSession = shotSession;
            AppVersion = appVersion;
        }

        public string AppVersion { get; }
    }
}
