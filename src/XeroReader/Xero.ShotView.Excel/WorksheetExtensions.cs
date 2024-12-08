namespace net.opgenorth.xero.shotview
{
    public static class WorksheetExtensions
    {
        public static IXLRow? FindRow(this IXLWorksheet ws, string title) =>
            ws.Rows().FirstOrDefault(row => title.Equals(row.GetString("A")));

        public static DateTime GetDateTimeUTC(this IXLRow row, string columnLetter = "B") => DateTime.Now;

        internal static string GetString(this IXLRow row, string columnLetter) =>
            row.Cell(columnLetter).GetText() ?? string.Empty;

        internal static string GetString(this IXLWorksheet ws, int row, int col) =>
            ws.Cell(row, col).GetText() ?? string.Empty;
    }
}
