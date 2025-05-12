namespace net.opgenorth.xero.Excel
{
    public interface IPersistShotSession
    {
        Task<int> DeleteSession(WorkbookSession session);
        Task UpsertSession(WorkbookSession session);
    }
}
