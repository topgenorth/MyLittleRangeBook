using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace MySimpleRangeLog.Database
{
    public static class SqliteHelper
    {
        /// <summary>
        ///     Creates a new <see cref="SqliteConnection" /> and opens it for usage.
        /// </summary>
        /// <remarks>
        ///     Ensure that the connection is disposed of after use.
        /// </remarks>
        /// <returns>The opened connection.</returns>
        internal static async Task<SqliteConnection> GetOpenConnectionAsync(string connectionString)
        {
            try
            {
                var connection = new SqliteConnection(connectionString);
                Log.Verbose("Using SQLite database {connectionString}", connectionString);

                await connection.OpenAsync();

                return connection;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Log.Logger.Error(e, "Failed to open database connection");

                throw;
            }
        }

        public static bool IsInMemoryDb(this SqliteConnection connection)
        {
            return connection.ConnectionString.Contains(":memory:", StringComparison.OrdinalIgnoreCase);
        }
    }
}
