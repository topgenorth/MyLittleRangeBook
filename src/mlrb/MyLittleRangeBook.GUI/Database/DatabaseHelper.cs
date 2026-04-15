using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.GUI.Helper;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using Serilog;
using JsonContextHelper = MyLittleRangeBook.GUI.Helper.JsonContextHelper;

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
        /// <param name="connection"></param>
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
