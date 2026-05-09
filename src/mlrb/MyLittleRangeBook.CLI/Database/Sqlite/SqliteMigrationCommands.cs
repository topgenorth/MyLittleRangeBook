using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Database.Sqlite;
using Spectre.Console;
using static MyLittleRangeBook.CLI.ReturnCodes;

namespace MyLittleRangeBook.CLI.Database.Sqlite
{
    /// <summary>
    ///     This class provides functionality for managing SQLite database migrations.
    /// </summary>
    [RegisterCommands("db")]
    [UsedImplicitly]
    public class SqliteMigrationCommands(ILogger logger, ICliDisplay cliDisplay, ISqliteHelper sqliteHelper)
    {
        const string MigrationsSql = "SELECT * FROM SchemaVersions ORDER BY Applied DESC";

        /// <summary>
        ///     Will return all the migrations that have been applied to the database.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("versions")]
        [UsedImplicitly]
        public async Task<int> DisplayMigrationVersionsToConsoleAsync(CancellationToken ct = default)
        {
            cliDisplay.PrintCommandHeader("Show migration versions");
            await RunMigrations(ct).ConfigureAwait(false);

            try
            {
                await using SqliteConnection connection = await sqliteHelper
                    .GetDatabaseConnectionAsync(ct)
                    .ConfigureAwait(false);
                await using var cmd = new SqliteCommand(MigrationsSql, connection);
                await using SqliteDataReader rdr = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

                Table table = new Table().Expand().BorderColor(Color.White);

                table.AddColumn("Row ID", col => col.Width(6).Centered());
                table.AddColumn("Script Name");
                table.AddColumn("Date Applied");

                while (await rdr.ReadAsync(ct).ConfigureAwait(false))
                {
                    string? schemaVersionId = rdr.IsDBNull(0) ? string.Empty : rdr.GetValue(0).ToString();
                    string scriptName = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                    string applied =
                        rdr.IsDBNull(2) ? string.Empty : rdr.GetValue(2).ToString() ?? string.Empty;
                    table.AddRow(schemaVersionId!, scriptName, applied);
                }

                cliDisplay.Console.Write(table);
                cliDisplay.PrintSuccess("Migration Versions listed.");

                return SUCCESS;
            }
            catch (Exception e)
            {
                cliDisplay.Console.WriteException(e);
                logger.Error(e, "Fail to display migrations.");

                return SQL_FAILED_TO_APPLY_MIGRATIONS;
            }
        }

        async Task RunMigrations(CancellationToken ct)
        {
            Result<bool> migrations = await sqliteHelper.ApplyDbupMigrationsAsync(ct).ConfigureAwait(false);
            if (migrations.IsFailed)
            {
                logger.Warning("There was a problem running the migrations. ");
            }
        }

        /// <summary>
        ///     Run the SQL statements to insert data into the database. The SQL file is not recorded as a migration.
        /// </summary>
        /// <param name="sqlfile">The SQL file to use.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("runsql")]
        [UsedImplicitly]
        // ReSharper disable once IdentifierTypo
        public async Task<int> RunSqlOnDatabase(string sqlfile, CancellationToken ct)
        {
            cliDisplay.PrintCommandHeader("Apply SQL to Database.");
            await RunMigrations(ct).ConfigureAwait(false);

            if (!File.Exists(sqlfile))
            {
                logger.Warning("SQL File {sqlFile} not found.", sqlfile);
                cliDisplay.PrintFailure($"Could not find the SQL file '{sqlfile}'.");

                return SQL_SCRIPT_FILE_NOT_FOUND;
            }

            try
            {
                cliDisplay.Console.MarkupLineInterpolated($"[green]✓ Loading SQL file {sqlfile}.[/]");
                string sql = await File.ReadAllTextAsync(sqlfile, ct).ConfigureAwait(false);

                Result<bool> result = await sqliteHelper.RunSqlOnDatabaseAsync(sql, ct).ConfigureAwait(false);
                if (result.IsFailed)
                {
                    logger.Error("Failed to run SQL '{sqlfile}'.", sqlfile);
                    cliDisplay.PrintFailure($"Failed to run SQL '{sqlfile}'.");

                    return SQL_FAILED_TO_RUN_SCRIPT;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to run SQL.");
                cliDisplay.PrintFailure($"Failed to run SQL '{e.Message}'.");

                return SQL_FAILED_TO_RUN_SCRIPT;
            }

            cliDisplay.PrintSuccess($"Successfully ran SQL '{sqlfile}'.");
            return SUCCESS;
        }

        /// <summary>
        ///     Ensures that all database schema migrations have been applied.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("migrate")]
        [UsedImplicitly]
        public async Task<int> MigrateSchemaAsync(CancellationToken ct = default)
        {
            cliDisplay.PrintCommandHeader("Applying Migrations");

            if (!File.Exists(sqliteHelper.DatabaseFile))
            {
                logger.Warning("SQLite database {file} not found.", sqliteHelper.DatabaseFile);
                cliDisplay.PrintFailure($"Could not find the SQLite database '{sqliteHelper.DatabaseFile}'.");

                return SQL_FAILED_TO_APPLY_MIGRATIONS;
            }

            Result<bool> migrationResult = await sqliteHelper.ApplyDbupMigrationsAsync(ct).ConfigureAwait(false);
            if (migrationResult.IsSuccess)
            {
                logger.Information("Migrations applied.");
                cliDisplay.PrintSuccess("Migrations applied.");
                return SUCCESS;
            }

            logger.Error("Failed to apply migrations.");
            cliDisplay.PrintFailure("Failed to apply migrations.");

            return SQL_FAILED_TO_APPLY_MIGRATIONS;
        }
    }
}
