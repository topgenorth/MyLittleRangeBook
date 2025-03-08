namespace net.opgenorth.xero.ShotView.Excel;

public interface IShotViewXslxAdapter : IDisposable
{
    string Filename { get; }

    string ToString();

    WorkbookSession? GetShotSession(int sheetNumber);
    IEnumerable<WorkbookSession> GetAllSessions();
}
