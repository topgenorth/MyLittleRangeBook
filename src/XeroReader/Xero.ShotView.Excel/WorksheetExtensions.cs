namespace net.opgenorth.xero.shotview
{
    public static class WorksheetExtensions
    {
        public static IXLRow? FindRowThatStartsWith(this IXLWorksheet ws, string title)
        {
            IXLRow? row = null;
            int rowCount = ws.Rows().Count();

            for (int i = 0; i < rowCount; i++)
            {
                var r = ws.Rows().ElementAt(i);
                var c = r.Cell("A");
                string t = c.GetText() ?? string.Empty;

                if (t.Equals(title, StringComparison.OrdinalIgnoreCase))
                {
                    row = r;
                    break;
                }
            }

            return row;
        }


        public static DateTime GetDateTimeUTC(this IXLRow row, string columnLetter = "B")
        {
            DateTime result = DateTime.UtcNow.ToUniversalTime();
            var t = row.GetString(columnLetter);
            if (DateTime.TryParse(t, out DateTime dt))
            {
                if (dt.Kind == DateTimeKind.Unspecified)
                {
                    dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
                }

                result = dt.ToUniversalTime();
            }

            return result;
        }

        internal static string GetString(this IXLRow row, string columnLetter) =>
            row.Cell(columnLetter).GetText() ?? string.Empty;

        internal static string GetString(this IXLWorksheet ws, int row, int col) =>
            ws.Cell(row, col).GetText() ?? string.Empty;
    }
}
