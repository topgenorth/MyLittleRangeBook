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
                IXLRow? r = ws.Rows().ElementAt(i);
                IXLCell? c = r.Cell("A");
                string t = c.GetText() ?? string.Empty;

                if (t.Equals(title, StringComparison.OrdinalIgnoreCase))
                {
                    row = r;

                    break;
                }
            }

            return row;
        }

        public static int? GetInteger(this IXLRow row, string columnLetter = "B")
        {
            IXLCell? c = row.Cell(columnLetter);
            if (c.TryGetValue(out int ival))
            {
                return ival;
            }

            if (c.TryGetValue(out float fval))
            {
                return Convert.ToInt32(fval);
            }

            string? t = c.GetText();
            if (string.IsNullOrWhiteSpace(t))
            {
                if (float.TryParse(t, out float fval2))
                {
                    return Convert.ToInt32(fval2);
                }
            }

            return null;
        }

        public static DateTime GetDateTimeUTC(this IXLRow row, string columnLetter = "B")
        {
            DateTime result = DateTime.UtcNow.ToUniversalTime();
            if (!row.Cell(columnLetter).TryGetValue(out DateTime dt))
            {
                return result;
            }

            if (dt.Kind == DateTimeKind.Unspecified)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            }

            result = dt.ToUniversalTime();

            return result;
        }

        internal static bool GetBool(this IXLRow row, string columnLetter = "B")
        {
            bool val;
            try
            {
                val = row.Cell(columnLetter).GetBoolean();
            }
            catch (InvalidCastException)
            {
                val = false;
            }

            return val;
        }

        internal static string GetString(this IXLRow row, string columnLetter) =>
            row.Cell(columnLetter).TryGetValue(out string val) ? val : null;


        internal static string GetString(this IXLWorksheet ws, int row, int col) =>
            ws.Cell(row, col).TryGetValue(out string val) ? val : null;
    }
}