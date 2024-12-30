
namespace net.opgenorth.xero.shotview
{
    public interface IShotViewXslxAdapter: IDisposable
    {

        string ToString();
        string Filename { get;  }

        WorkbookSession? GetShotSession(int sheetNumber);
        IEnumerable<WorkbookSession> GetAllSessions();
    }
}
