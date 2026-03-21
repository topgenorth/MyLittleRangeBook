using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MySimpleRangeLog.Helper;
using MySimpleRangeLog.Models;
using MySimpleRangeLog.Services;
using Serilog;

namespace MySimpleRangeLog.Database
{
    public static class DatabaseHelper
    {
        // A flag that indicates if the DB is yet initialized.
        static bool _initialized;


        static readonly Firearm[] TestFirearms =
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

        static readonly SimpleRangeEvent[] TestRangeEvents =
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

        static readonly string[] DATABASE_SCRIPTS;

        static DatabaseHelper()
        {
            DATABASE_SCRIPTS =
            [
                "MySimpleRangeLog.Database.scripts.001-SimpleRangeEvents-schema.sql",
                "MySimpleRangeLog.Database.scripts.002-SimpleRangeEvents-index.sql",
                "MySimpleRangeLog.Database.scripts.004-Images-schema.sql",
                "MySimpleRangeLog.Database.scripts.005-SimpleRangeEventImages-schema.sql",
                "MySimpleRangeLog.Database.scripts.006-Firearms-schema.sql"
            ];
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

                await EnsureSQLiteDatabaseExists(connection, connection.IsInMemoryDb());

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

        static async Task<bool> DoesTableExistAsync(SqliteConnection connection, string tableName)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT COUNT(name) as TableCount FROM sqlite_master WHERE type='table' AND name=@tableName";
            cmd.Parameters.AddWithValue("@tableName", tableName);

            var result = await cmd.ExecuteScalarAsync();

            if (result is null)
            {
                return false;
            }

            return (long)result != 0;
        }

        /// <summary>
        ///     Ensures that all tables are created and the database is ready to be used.
        /// </summary>
        /// <param name="connection">the connection to use</param>
        /// <param name="force">the creating will be skipped by default, unless you set this parameter to true.</param>
        // ReSharper disable once InconsistentNaming
        public static async Task EnsureSQLiteDatabaseExists(SqliteConnection connection, bool force = false)
        {
            if (_initialized && !force)
            {
                return;
            }

            if (await DoesTableExistAsync(connection, "SimpleRangeEvents"))
            {
                await PopulateSampleDataIfNeeded(connection);

                Log.Verbose("Table 'SimpleRangeEvents' already exists, skipping initialization.");
                _initialized = true;

                return;
            }

            var a = Assembly.GetAssembly(typeof(Program));
            foreach (var script in DATABASE_SCRIPTS)
            {
                try
                {
                    var sql = await a!.ReadEmbeddedTextFileAsync(script);
                    await connection.ExecuteAsync(sql);
                    Log.Verbose("Executed script: {Script}", script);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to execute script {script}", script);

                    _initialized = false;

                    return;
                }
            }

            await PopulateSampleDataIfNeeded(connection);

            // For in memory DataSource, we cannot set the _init flag to true. 
            _initialized = App.Services.GetRequiredService<IDatabaseService>().GetConnectionString() != ":memory:";
        }

        static async Task PopulateSampleDataIfNeeded(SqliteConnection connection)
        {
            var eventCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM SimpleRangeEvents");
            if (eventCount > 0)
            {
                return;
            }

            if (Design.IsDesignMode)
            {
                await connection.ExecuteAsync(
                    """
                    REPLACE INTO SimpleRangeEvents 
                        (Id, EventDate, FirearmName, RangeName, RoundsFired, AmmoDescription, Notes, Created, Modified ) 
                    VALUES
                        (@Id, @EventDate, @FirearmName, @RangeName, @RoundsFired, @AmmoDescription, @Notes, @Created, @Modified);
                    """, TestRangeEvents);
            }
            // Populate some data for the designer and our unit tests if it has none yet.
            else if (connection.ConnectionString.Contains(":memory:"))
            {
                var sql = await typeof(DatabaseHelper).Assembly!.ReadEmbeddedTextFileAsync(
                    "003-SimpleRangeEvents-data.sql");
                await connection.ExecuteAsync(sql);
            }
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
