using System.Data;
using DbUp;
using DbUp.Engine;
using FluentResults;
using Microsoft.Extensions.Configuration;
using NanoidDotNet;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///     A helper class for managing SQLite database connections, initialization, and configuration.
    ///     Provides methods for setting up the database environment, getting connections, and generating connection strings.
    /// </summary>
    public class SqliteHelper : ISqliteHelper, IDatabaseHelper
    {
        readonly string _connectionString;
        readonly ILogger _logger;

        public SqliteHelper(ILogger logger, string connectionString)
        {
            _logger = logger;

            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.Warning("SQLite connection string is empty!");
                DatabaseFile = string.Empty;
                _connectionString = string.Empty;
            }

            var builder = new SqliteConnectionStringBuilder(connectionString) { Mode = SqliteOpenMode.ReadWriteCreate };
            _connectionString = builder.ConnectionString;
            DatabaseFile = builder.DataSource;
        }

        public SqliteHelper(ILogger logger, IConfiguration configuration) : this(logger,
            GetSqliteConnectionString(configuration))
        {
        }

        async Task<IDbConnection> IDatabaseHelper.GetDatabaseConnectionAsync(CancellationToken cancellationToken)
        {
            return await GetDatabaseConnectionAsync(cancellationToken);
        }


        public string DatabaseFile { get; }

        /// <summary>
        ///     Creates a new <see cref="SqliteConnection" /> and opens it for usage.
        /// </summary>
        /// <remarks>
        ///     Ensure that the connection is disposed of after use. This will also adds the custom functions nanoid() and
        ///     utcnow().
        /// </remarks>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The opened connection.</returns>
        public async Task<SqliteConnection> GetDatabaseConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SqliteConnection(_connectionString);
            connection.CreateFunction("nanoid", () => Nanoid.Generate());
            connection.CreateFunction("utcnow", () => DateTimeOffset.UtcNow.ToString("O"));
            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        public async Task<Result<bool>> CreateSqliteDatabaseAsync(string sqliteDatabaseFile,
            CancellationToken cancellationToken)
        {
            if (File.Exists(sqliteDatabaseFile))
            {
                // TODO [TO20260418] Add a reason that the database exists.
                return Result.Ok(true);
            }

            // [TO20260418]This will create the database
            await using SqliteConnection conn = await GetDatabaseConnectionAsync(cancellationToken);
            await conn.CloseAsync();

            await ApplyDbupMigrationsAsync(cancellationToken);

            return Result.Ok(true);
        }

        public async Task<Result<bool>> ApplyDbupMigrationsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                UpgradeEngine? upgrader = DeployChanges.To
                    .SqliteDatabase(_connectionString)
                    .WithScriptsEmbeddedInAssembly(GetType().Assembly)
                    .LogToConsole()
                    .Build();

                DatabaseUpgradeResult? migrationResult = upgrader.PerformUpgrade();
                if (migrationResult.Successful)
                {
                    return Result.Ok(true);
                }

                Error? err = new Error("Could not apply database migrations.")
                        .WithMetadata("Errors", migrationResult.Error)
                        .WithMetadata("Database", _connectionString)
                    ;

                return Result.Fail<bool>(err).WithValue(false);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unexpected error occurred while applying database migrations.");
                Error? err = new Error("Unexpected error occurred while applying database migrations.")
                        .CausedBy(e)
                        .WithMetadata("Exception", e)
                        .WithMetadata("Database", _connectionString)
                    ;

                return Result.Fail<bool>(err).WithValue(false);
            }
        }

        public async Task<Result<bool>> RunSqlOnDatabaseAsync(string sql, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sql))
            {
                _logger.Information("There is no SQL - nothing to do.");

                return Result.Ok(true);
            }

            try
            {
                await using SqliteConnection connection = await GetDatabaseConnectionAsync(cancellationToken);
                await using var cmd = new SqliteCommand(sql, connection);
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                return Result.Ok(true);
            }
            catch (Exception e)
            {
                Error? err = new Error("Failed to run SQL.").CausedBy(e);
                err.Metadata.Add("SQL", sql);
                err.Metadata.Add("Connection", _connectionString);

                return Result.Fail<bool>(err).WithValue(false);
            }
        }

        public override string ToString()
        {
            return _connectionString;
        }

        static string GetSqliteConnectionString(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("SqliteConnection");

            return string.IsNullOrEmpty(connectionString)
                ? throw new InvalidOperationException("SQLite connection string 'SqliteConnection' is not configured.")
                : connectionString!;
        }
    }
}
