using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Database.Sqlite;
using Spectre.Console;
using static MyLittleRangeBook.CLI.ReturnCodes;

namespace MyLittleRangeBook.CLI.Database
{
    /// <summary>
    ///     This class provides functionality for managing SQLite database migrations.
    /// </summary>
    [RegisterCommands("db")]
    [UsedImplicitly]
    public class SqliteMigrator(ILogger logger, ICliDisplay cliDisplay, ISqliteHelper sqliteHelper)
    {
        const string MigrationsSql = "SELECT * FROM SchemaVersions ORDER BY Applied DESC";
        /// <summary>
        ///     Will return all the migrations that have been applied to the database.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("versions")]
        [UsedImplicitly]
        public async Task<int> DisplayMigrationVersionsToConsoleAsync(string file, CancellationToken cancellationToken)
        {

            Result<bool> migrations = await sqliteHelper.ApplyDbupMigrationsAsync(cancellationToken);

            // TODO [TO20260418] Improve the CLI output.
            cliDisplay.WriteHeader("Show migration versions");
            if (!File.Exists(file))
            {
                logger.Warning("File {file} not found.", file);
                cliDisplay.WriteFailure($"Could not find the database '{file}'.");

                return DATABASE_FILE_NOT_FOUND;
            }

            try
            {
                int result = await cliDisplay.RunStatusAsync(
                    "Retrieving migration versions...",
                    async ct =>
                    {
                        await using SqliteConnection connection = await sqliteHelper.GetDatabaseConnectionAsync(ct);
                        await using var cmd =
                            new SqliteCommand(MigrationsSql, connection);
                        await using SqliteDataReader rdr = await cmd.ExecuteReaderAsync(ct);

                        var table = new Table();
                        table.AddColumn("Migration ID");
                        table.AddColumn("Script Name");
                        table.AddColumn("Date Applied");

                        while (await rdr.ReadAsync(ct))
                        {
                            string? schemaVersionId = rdr.IsDBNull(0) ? string.Empty : rdr.GetValue(0).ToString();
                            string scriptName = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                            string applied = rdr.IsDBNull(2) ? string.Empty : rdr.GetValue(2).ToString() ?? string.Empty;
                            table.AddRow(schemaVersionId!, scriptName, applied);
                        }

                        cliDisplay.Console.Write(table);
                        cliDisplay.WriteSuccess("Migration Versions listed.");

                        return SUCCESS;
                    },
                    cancellationToken);

                return result;
            }
            catch (Exception e)
            {
                cliDisplay.Console.WriteException(e);

                return FAILED_TO_APPLY_MIGRATIONS;
            }

        }

        /// <summary>
        ///     Run the SQL statements to insert data into the database. The SQL file is not recorded as a migration.
        /// </summary>
        /// <param name="file">The database file.</param>
        /// <param name="sqlfile">The SQL file to use.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("runsql")]
        [UsedImplicitly]
        // ReSharper disable once IdentifierTypo
        public async Task<int> RunSqlOnDatabase(string file, string sqlfile, CancellationToken cancellationToken)
        {
            cliDisplay.WriteHeader("Apply SQL to Database.");

            if (!File.Exists(file))
            {
                logger.Warning("File {file} not found.", file);
                cliDisplay.WriteFailure($"Could not find the database '{file}'.");

                return DATABASE_FILE_NOT_FOUND;
            }

            if (!File.Exists(sqlfile))
            {
                logger.Warning("SQL File {sqlFile} not found.", sqlfile);
                cliDisplay.WriteFailure($"Could not find the SQL file '{sqlfile}'.");

                return SQL_FILE_NOT_FOUND;
            }

            Task<Result<bool>> migrations = sqliteHelper.ApplyDbupMigrationsAsync(cancellationToken);


            int result = await cliDisplay.RunStatusAsync("Loading SQL file...",
                async ct =>
                {
                    void WriteSuccess()
                    {
                        cliDisplay.WriteSuccess("SQL file applied to database.");
                    }

                    try
                    {
                        cliDisplay.Console.MarkupLineInterpolated($"[green]✓ Loading SQL file {sqlfile}.[/]");
                        string sql = await File.ReadAllTextAsync(sqlfile, ct);

                        Result<bool> result= await sqliteHelper.RunSqlOnDatabaseAsync(sql, ct);
                        if (result.IsSuccess)
                        {
                            WriteSuccess();
                            return SUCCESS;
                        }

                        return FAILED_TO_RUN_SQL;

                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to run SQL.");
                        cliDisplay.WriteFailure($"Failed to run SQL '{e.Message}'.");

                        return FAILED_TO_RUN_SQL;
                    }
                }, cancellationToken);

            return result;
        }

        /// <summary>
        ///     Ensures that all database schema migrations have been applied.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("migrate")]
        [UsedImplicitly]
        public async Task<int> MigrateSchemaAsync(CancellationToken cancellationToken = default)
        {
            // TODO [TO20260418] Improve the CLI output.
            cliDisplay.WriteHeader("Applying Migrations");

            if (!File.Exists(sqliteHelper.DatabaseFile))
            {
                logger.Warning("SQLite database {file} not found.", sqliteHelper.DatabaseFile);
                cliDisplay.Console.MarkupLineInterpolated(
                    $"[bold warn]✗ Could not find '{sqliteHelper.DatabaseFile}'; database will be created.[/]");

                return DATABASE_FILE_NOT_FOUND;
            }

            Result<bool> migrationResult =await sqliteHelper.ApplyDbupMigrationsAsync(cancellationToken);
            if (migrationResult.IsSuccess)
            {
                cliDisplay.WriteSuccess("Migrations applied.");
                return SUCCESS;
            }

            cliDisplay.WriteFailure("Failed to apply migrations.");
            return FAILED_TO_APPLY_MIGRATIONS;
        }
    }
}
