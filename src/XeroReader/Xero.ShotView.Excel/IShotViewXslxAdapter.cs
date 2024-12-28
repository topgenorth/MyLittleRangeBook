
namespace net.opgenorth.xero.shotview
{
    public interface IShotViewXslxAdapter: IDisposable
    {
        void Dispose();
        string ToString();
        string Filename { get;  }

        WorkbookSession? GetShotSession(int sheetNumber);
        IEnumerable<WorkbookSession> GetAllSessions();
    }
}
