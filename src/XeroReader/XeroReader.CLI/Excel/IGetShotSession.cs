namespace net.opgenorth.xero.Excel
{
    public interface IGetShotSession
    {
        string Filename { get; }

        Task<WorkbookSession> GetSessionByName(string sheetName);
    }
}
