
namespace net.opgenorth.xero.shotview
{
    internal  static class WorksheetExtensions
    {
        public static string GetString(this IXLWorksheet ws, int row, int col)
        {
            return ws.Cell(row, col).GetText() ?? string.Empty;
        }
    }
}
