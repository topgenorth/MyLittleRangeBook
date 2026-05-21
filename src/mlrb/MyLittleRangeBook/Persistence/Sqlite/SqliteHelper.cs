using System.Data;
using System.Text.Json.Nodes;
using Dapper;
using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.Database;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Persistence.Sqlite
{
    /// <summary>
    ///     An enumeration that represents the types of files that can be saved in SQlite.
    /// </summary>
    public enum SqliteFileTable
    {
        FitFiles,
        ShotViewCsvFiles,
        ImageFiles
    }


    /// <summary>
    ///     A helper class for managing SQLite database connections, initialization, and configuration.
    ///     Provides methods for setting up the database environment, getting connections, and generating connection strings.
    /// </summary>
    public class SqliteHelper : ISqliteHelper, IDatabaseHelper
    {
        /// <summary>
        ///     Kilobytes. If a file is larger than this warning threshold, it may cause performance issues when writing to the
        ///     database.
        /// </summary>
        public const int FILE_LENGTH_THRESHOLD = 100 * 1024;

        readonly string _connectionString;
        readonly ILogger _logger;

        public SqliteHelper(ILogger logger, string connectionString)
        {
            _logger = logger;

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.Warning("SQLite connection string is empty!");
                DatabaseFile = string.Empty;
                _connectionString = string.Empty;
            }
            else
            {
                var builder = new SqliteConnectionStringBuilder(connectionString)
                {
                    Mode = SqliteOpenMode.ReadWriteCreate
                };
                _connectionString = builder.ConnectionString;
                DatabaseFile = builder.DataSource;
            }
        }

        public SqliteHelper(ILogger logger, IConfiguration configuration) :
            this(logger, configuration.GetSqliteConnectionString())
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
            connection.AddFunctions();
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await connection.ExecuteAsync("PRAGMA foreign_keys = ON;").ConfigureAwait(false);

            return connection;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task<Result> ApplyDbupMigrationsAsync(CancellationToken cancellationToken = default)
        {
            Result result;
            try
            {
                UpgradeEngineBuilder? ueb = DeployChanges.To
                    .SqliteDatabase(_connectionString)
                    .WithScriptsEmbeddedInAssembly(GetType().Assembly);
                if (!EnvironmentExtensions.IsProduction)
                {
                    ueb.LogToConsole();
                    ueb.LogScriptOutput();
                }

                UpgradeEngine? upgrader = ueb.Build();

                DatabaseUpgradeResult? migrationResult = upgrader.PerformUpgrade();
                if (migrationResult.Successful)
                {
                    // TODO [TO20260419] Include a reason.
                    result = new Result();
                }
                else
                {
                    Error? err = new Error("Could not apply database migrations.")
                        .WithMetadata("Errors", migrationResult.Error)
                        .WithMetadata("Database", _connectionString);

                    result = new Result().WithError(err);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unexpected error occurred while applying database migrations.");
                Error? err = new Error("Unexpected error occurred while applying database migrations.")
                        .CausedBy(e)
                        .WithMetadata("Exception", e)
                        .WithMetadata("Database", _connectionString)
                    ;

                result = new Result().WithError(err);
            }

            return result;
        }

        public async Task<Result> RunSqlOnDatabaseAsync(string sql, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sql))
            {
                _logger.Information("There is no SQL - nothing to do.");

                return Result.Ok();
            }

            try
            {
                await using SqliteConnection connection = await GetDatabaseConnectionAsync(cancellationToken);
                await using var cmd = new SqliteCommand(sql, connection);
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                return Result.Ok();
            }
            catch (SqliteException sqe)
            {
                Error? err = new Error(sqe.Message)
                    .CausedBy(sqe)
                    .WithMetadata("SQL", sql)
                    .WithMetadata("SQLiteErrorCode", sqe.ErrorCode)
                    .WithMetadata("SQLiteExtendedErrorCode", sqe.SqliteExtendedErrorCode)
                    .WithMetadata("Connection", _connectionString);

                return Result.Fail(err);
            }
            catch (Exception e)
            {
                Error? err = new Error("Unexpected error running SQL on database..").CausedBy(e);
                err.Metadata.Add("SQL", sql);
                err.Metadata.Add("Connection", _connectionString);

                return Result.Fail(err);
            }
        }

        public async Task<Result<(string id, long rowId)>> WriteFileToTableAsync(SqliteConnection conn,
            SqliteFileTable table,
            string fileName,
            byte[] fileContents,
            CancellationToken cancellationToken)
        {
            string sql = table switch
            {
                SqliteFileTable.FitFiles => """
                                            INSERT INTO FitFiles (Id, FileName, Contents)
                                            VALUES (@Id, @FileName, @FileContents)
                                            RETURNING rowid;
                                            """,
                _ => throw new ArgumentOutOfRangeException(nameof(table), $"Unsupported table: {table}")
            };

            switch (fileContents.Length)
            {
                case 0:
                    return Result.Ok((string.Empty, 0L))
                        .WithReason(
                            new Success("File contents are empty - nothing to write.").WithMetadata("Table", table));
                case > FILE_LENGTH_THRESHOLD:
                    _logger.Warning(
                        "File contents are larger than 100KB. This may cause performance issues when writing to the database. Table: {Table}, Size: {Size} bytes",
                        table, fileContents.Length);

                    break;
            }

            var id = new MlrbId().ToString();
            try
            {
                // TODO [TO20260503] It's possible to duplicate file contents; maybe file name should be unique in the database?
                var cmd = new SqliteCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@FileContents", fileContents);
                cmd.Parameters.AddWithValue("@FileName", fileName);

                object? l = await cmd.ExecuteScalarAsync(cancellationToken);
                long rowId = l is null ? -1 : Convert.ToInt64(l);

                return Result.Ok((id, rowId));
            }
            catch (Exception e)
            {
                Error? err = new Error($"Failed to write file to table {table}.").CausedBy(e);
                err.Metadata.Add("Table", table);
                err.Metadata.Add("FileName", fileName);

                return Result.Fail(err);
            }
        }

        /// <summary>
        ///     Creates the SQlite database if it does not exist. Applies migrations if necessary.
        /// </summary>
        /// <param name="sqliteDatabaseFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<bool>> CreateSqliteDatabaseAsync(string sqliteDatabaseFile,
            CancellationToken cancellationToken = default)
        {
            if (File.Exists(sqliteDatabaseFile))
            {
                _logger.Verbose("Database exists {0}", sqliteDatabaseFile);
            }
            else
            {
                await using SqliteConnection conn = await GetDatabaseConnectionAsync(cancellationToken);
                await conn.CloseAsync();
                _logger.Debug("Created database {0}", sqliteDatabaseFile);
            }

            Result<bool> result = await ApplyDbupMigrationsAsync(cancellationToken);

            return result.IsSuccess ? Result.Ok(true) : result;
        }

        public async Task<Result<bool>> RunSqlOnDatabaseAsync(SqliteTransaction trans,
            SqliteCommand sqliteCommand,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(trans);

            try
            {
                sqliteCommand.Transaction = trans;
                await sqliteCommand.ExecuteNonQueryAsync(cancellationToken);

                return Result.Ok(true);
            }
            catch (SqliteException sqe)
            {
                Error? err = new Error(sqe.Message)
                    .CausedBy(sqe)
                    .WithMetadata("SQL", sqliteCommand.CommandText)
                    .WithMetadata("SQLiteErrorCode", sqe.ErrorCode)
                    .WithMetadata("SQLiteExtendedErrorCode", sqe.SqliteExtendedErrorCode)
                    .WithMetadata("Connection", _connectionString);

                return Result.Fail<bool>(err).WithValue(false);
            }
            catch (Exception e)
            {
                Error? err = new Error("Unexpected error running SQL on database.").CausedBy(e);
                err.Metadata.Add("SQL", sqliteCommand.CommandText);
                err.Metadata.Add("Connection", _connectionString);

                return Result.Fail<bool>(err).WithValue(false);
            }
        }

        public async Task<Result<bool>> UpdateSqliteDatabasePathAsync(string sqliteDatabaseName,
            string? appSettingsFile = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sqliteDatabaseName))
            {
                var err = new Error("SQLite database name is empty.");

                return Result.Fail<bool>(err).WithValue(false);
            }

            if (string.IsNullOrWhiteSpace(appSettingsFile))
            {
                appSettingsFile = DatabaseFile;
            }

            var appSettings = new FileInfo(appSettingsFile);
            if (!appSettings.Exists)
            {
                var err = new Error($"Could not find appsettings.json at {appSettingsFile}.");

                return Result.Fail<bool>(err).WithValue(false);
            }

            using StreamReader reader = appSettings.OpenText();
            string jsonBody = await reader.ReadToEndAsync(cancellationToken);

            var node = JsonNode.Parse(jsonBody);
            if (node is null)
            {
                Error? err = new Error($"Could not parse appsettings.json at {appSettingsFile}.")
                    .WithMetadata("Json", jsonBody)
                    .WithMetadata("File", appSettingsFile);

                return Result.Fail<bool>(err).WithValue(false);
            }

            var b = new SqliteConnectionStringBuilder
            {
                DataSource = sqliteDatabaseName, Mode = SqliteOpenMode.ReadWriteCreate
            };

            // [TO20260414] Just wondering if the Mode should be set to ReadWriteCreate?
            node["ConnectionStrings"]!["SqliteConnection"] = b.ConnectionString;

            var success = new Success("Updated appsettings.json with SQLite database connection string.");
            success.Metadata.Add("File", appSettingsFile);
            success.Metadata.Add("ConnectionString", b.ConnectionString);

            return Result.Ok(true).WithSuccess(success);
        }

        public override string ToString()
        {
            return _connectionString;
        }
    }
}
