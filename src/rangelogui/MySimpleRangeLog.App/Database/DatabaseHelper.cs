using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Database.Sqlite;
using MySimpleRangeLog.Helper;
using MySimpleRangeLog.Models;
using MySimpleRangeLog.Services;
using Serilog;

namespace MySimpleRangeLog.Database
{
    public static class DatabaseHelper
    {
        internal static readonly Firearm[] TestFirearms =
        [
            new()
            {
                RowId = 1,
                Id = "NANOID-1",
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow,
                Name = "STAG-10",
                Notes = null
            },
            new()
            {
                Id = "NANOID-2",
                RowId = 2,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow,
                Name = "Ruger 10/22",
                Notes = "Mapleseed rifle."
            }
        ];

        internal static readonly SimpleRangeEvent[] TestRangeEvents =
        [
            new()
            {
                Id = "NANOID-3",
                RowId = 1,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow,
                EventDate = new DateTime(2024, 03, 12),
                FirearmName = "Ruger 10/122",
                RangeName = "CHAS",
                RoundsFired = 350,
                AmmoDescription = "CCI SV",
                Notes = "Sample Event #1"
            },
            new()
            {
                Id = "NANOID-4",
                RowId = 2,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow,
                EventDate = new DateTime(2025, 03, 12),
                FirearmName = "Tikka T3",
                RangeName = "SPFGA",
                RoundsFired = 10,
                AmmoDescription = "178gr Hornady BTHP;39.1gr IMR-3031;CCI #200; Federal Brass;2.902 COAL",
                Notes = "Sample event #2"
            }
        ];


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
            if (connection.IsInMemoryDb())
            {
                return TestRangeEvents;
            }

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
        ///     Syncs the database with the underlying data store if needed.
        /// </summary>
        /// <remarks>
        ///     Wasm uses IndexedDB to store the data. This needs to be synced.
        ///     This helper will do it for us. If you don't need the WASM-target, this can be omitted.
        /// </remarks>
        public static async Task SyncUnderlyingDatabaseAsync()
        {
            await App.Services.GetRequiredService<IDatabaseService>().SaveAsync();
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
            if (connection.IsInMemoryDb())
            {
                return TestFirearms;
            }

            const string sql = """
                               SELECT *
                               FROM Firearms 
                               ORDER BY Name;
                               """;

            Firearm[] firearms;

            return (await connection.QueryAsync<Firearm>(sql)).ToArray();
        }
    }
}
