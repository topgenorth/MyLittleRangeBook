namespace net.opgenorth.xero.Excel
{
    public interface IShotViewXslxAdapter : IDisposable
    {
        string Filename { get; }

        string ToString();

        WorkbookSession? GetShotSession(int sheetNumber);
        IEnumerable<WorkbookSession> GetAllSessions();
    }
}
