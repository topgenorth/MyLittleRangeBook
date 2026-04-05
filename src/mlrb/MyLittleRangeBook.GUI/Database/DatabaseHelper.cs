using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.GUI.Helper;
using MyLittleRangeBook.GUI.Models;
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

        public static async Task<IEnumerable<SimpleRangeEvent>> GetSimpleRangeEventsAsync(SqliteConnection connection,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                               SELECT *
                               FROM SimpleRangeEvents 
                               ORDER BY EventDate, FirearmName, RangeName;
                               """;

            SimpleRangeEvent[] rangeEvents;

            try
            {
                rangeEvents = (await connection.QueryAsync<SimpleRangeEvent>(sql, cancellationToken)).ToArray();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to retrieve simple range events from database");
                rangeEvents = [];
            }

            return rangeEvents;
        }

        /// <summary>
        ///     Saves a JSON representation of the entire database into the provided Stream.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="targetStream">The target Stream to save to</param>
        /// <param name="cancellationToken"></param>
        public static async Task ExportToJsonAsync(SqliteConnection connection,
            Stream targetStream,
            CancellationToken cancellationToken = default)
        {
            var dto = new DatabaseDto
            {
                SimpleRangeEvents = (await GetSimpleRangeEventsAsync(connection, cancellationToken)).ToArray()
            };
            await JsonSerializer.SerializeAsync(targetStream, dto, JsonContextHelper.Default.DatabaseDto,
                cancellationToken);
        }

        public static async Task<IEnumerable<Firearm>> GetFirearmsAsync(SqliteConnection connection,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                               SELECT *
                               FROM Firearms 
                               ORDER BY Name;
                               """;
            return (await connection.QueryAsync<Firearm>(sql)).ToArray();
        }
    }
}
