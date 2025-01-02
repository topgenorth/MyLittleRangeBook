namespace net.opgenorth.xero.shotview;

public interface IShotViewXslxAdapter : IDisposable
{
    string Filename { get; }
    string ToString();

    WorkbookSession? GetShotSession(int sheetNumber);
    IEnumerable<WorkbookSession> GetAllSessions();
}
