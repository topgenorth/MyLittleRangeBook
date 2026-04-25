using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook.GUI.Database
{
    public static class DatabaseHelper
    {
        [Obsolete("Use ISqliteHelper.OpenSqliteConnectionToFileAsync instead", true)]
        internal static async Task<SqliteConnection> GetOpenConnectionAsync(ISqliteHelper dbService,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }


        public static bool IsInMemoryDb(this SqliteConnection connection)
        {
            return connection.ConnectionString.Contains(":memory:", StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        ///     Saves a JSON representation of the entire database into the provided Stream.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="targetStream">The target Stream to save to</param>
        /// <param name="cancellationToken"></param>
        public static async Task ExportToJsonAsync(ISimpleRangeLogService repo,
            Stream targetStream,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            // var result = await repo.GetSimpleRangeEventsAsync(connection, cancellationToken);
            //
            // if (result.IsFailed)
            // {
            //     return;
            // }
            //
            // var dto = new DatabaseDto
            // {
            //     SimpleRangeEvents = result.Value.ToArray();
            // };
            // await JsonSerializer.SerializeAsync(targetStream, dto, JsonContextHelper.Default.DatabaseDto,
            //     cancellationToken);
        }
    }
}
