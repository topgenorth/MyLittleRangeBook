using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MySimpleRangeLog.Helper;
using MySimpleRangeLog.Models;
using MySimpleRangeLog.Services;
using NanoidDotNet;
using Serilog;

namespace MySimpleRangeLog.Database
{
    public static class DatabaseHelper
    {
        private static readonly Firearm[] TestFirearms =
        [
            new()
            {
                Id = 1,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow,
                Name = "STAG-10",
                Notes = null
            },
            new()
            {
                Id = 2,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow,
                Name = "Ruger 10/22",
                Notes = "Mapleseed rifle."
            }
        ];

        private static readonly SimpleRangeEvent[] TestRangeEvents =
        [
            new()
            {
                Id = 1,
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
                Id = 2,
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


        static DatabaseHelper()
        {
        }

        /// <summary>
        ///     Creates a new <see cref="SqliteConnection" /> and opens it for usage.
        /// </summary>
        /// <remarks>
        ///     Ensure that the connection is disposed of after use.
        /// </remarks>
        /// <returns>The opened connection.</returns>
        internal static async Task<SqliteConnection> GetOpenConnectionAsync(IDatabaseService dbService)
        {
            try
            {
                var connectionString = dbService.GetConnectionString();
                var connection = new SqliteConnection(connectionString);

                await connection.OpenAsync();

                connection.CreateFunction("nanoid", () => Nanoid.Generate());
                connection.CreateFunction("utcnow", () => DateTimeOffset.UtcNow.ToString("O"));

                Log.Verbose("Using SQLite database {connectionString}", connectionString);

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

        public static async Task<IEnumerable<SimpleRangeEvent>> GetSimpleRangeEventsAsync()
        {
            const string sql = """
                               SELECT *
                               FROM SimpleRangeEvents 
                               ORDER BY EventDate, FirearmName, RangeName;
                               """;

            SimpleRangeEvent[] rangeEvents;
            await using var connection =
                await GetOpenConnectionAsync(App.Services.GetRequiredService<IDatabaseService>());
            if (connection.IsInMemoryDb())
            {
                return TestRangeEvents;
            }

            try
            {
                rangeEvents = (await connection.QueryAsync<SimpleRangeEvent>(sql)).ToArray();
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
        /// <param name="targetStream">The target Stream to save to</param>
        public static async Task ExportToJsonAsync(Stream targetStream)
        {
            var dto = new DatabaseDto { SimpleRangeEvents = (await GetSimpleRangeEventsAsync()).ToArray() };

            await JsonSerializer.SerializeAsync(targetStream, dto,
                JsonContextHelper.Default.DatabaseDto);
        }

        public static async Task<IEnumerable<Firearm>> GetFirearmsAsync()
        {
            const string sql = """
                               SELECT *
                               FROM Firearms 
                               ORDER BY Name;
                               """;

            Firearm[] firearms;
            await using var connection =
                await GetOpenConnectionAsync(App.Services.GetRequiredService<IDatabaseService>());
            if (connection.IsInMemoryDb())
            {
                return TestFirearms;
            }

            try
            {
                firearms = (await connection.QueryAsync<Firearm>(sql)).ToArray();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to retrieve simple range events from database");
                firearms = [];
            }

            return firearms;
        }
    }
}