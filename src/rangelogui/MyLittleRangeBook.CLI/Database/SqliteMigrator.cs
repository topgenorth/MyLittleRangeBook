using ConsoleAppFramework;
using DbUp;
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
    [RegisterCommands("schema")]
    [UsedImplicitly]
    public class SqliteMigrator(ILogger logger, ICliDisplay cliDisplay, ISqliteHelper sqliteHelper)
    {
        /// <summary>
        ///     Will return all the migrations that have been applied to the database.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("version")]
        [UsedImplicitly]
        public async Task<int> MigrationVersionAsync(string file, CancellationToken cancellationToken)
        {
            cliDisplay.WriteHeader("Migration Version");
            if (!File.Exists(file))
            {
                logger.Warning("File {file} not found.", file);
                cliDisplay.WriteFailure($"Could not find the database '{file}'.");

                return DATABASE_FILE_NOT_FOUND;
            }

            var result = await cliDisplay.RunStatusAsync(
                "Importing FIT data...",
                async ct =>
                {
                    await using var connection = await sqliteHelper.OpenSqliteConnectionToFileAsync(file, ct);
                    await using var cmd = new SqliteCommand("SELECT * FROM SchemaVersions ORDER BY Applied", connection);
                    await using var rdr = await cmd.ExecuteReaderAsync(ct);

                    var table = new Table();
                    table.AddColumn("ID");
                    table.AddColumn("Script Name");
                    table.AddColumn("Date Applied");

                    while (await rdr.ReadAsync(ct))
                    {
                        var schemaVersionId = rdr.IsDBNull(0) ? string.Empty : rdr.GetValue(0).ToString();
                        var scriptName = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                        var applied = rdr.IsDBNull(2) ? string.Empty : rdr.GetValue(2).ToString() ?? string.Empty;
                        table.AddRow(schemaVersionId!, scriptName, applied);
                    }

                    cliDisplay.Console.Write(table);
                    cliDisplay.WriteSuccess("Migration Versions listed.");

                    return SUCCESS;
                },
                cancellationToken);

            return result;
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


            var result = await cliDisplay.RunStatusAsync("Loading SQL file...",
                async ct =>
                {
                    void WriteSuccess()
                    {
                        cliDisplay.WriteSuccess("SQL file applied to database.");
                    }

                    try
                    {
                        cliDisplay.Console.MarkupLineInterpolated($"[green]✓ Loading SQL file {sqlfile}.[/]");
                        var sql = await File.ReadAllTextAsync(sqlfile, ct);

                        if (string.IsNullOrWhiteSpace(sql))
                        {
                            logger.Information("SQL File {sqlFile} is empty - nothing done.", sqlfile);
                            WriteSuccess();

                            return SUCCESS;
                        }


                        await using var connection = await sqliteHelper.OpenSqliteConnectionToFileAsync(file, ct);
                        await using var cmd = new SqliteCommand(sql, connection);
                        await cmd.ExecuteNonQueryAsync(ct);
                        WriteSuccess();

                        return SUCCESS;
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
        ///     Ensures that all database schema migrations are applied to the specified SQLite database file.
        /// </summary>
        /// <param name="file">The full path to the SQLite database file.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("migrate")]
        [UsedImplicitly]
        public async Task<int> MigrateSchemaAsync(string file, CancellationToken cancellationToken)
        {
            cliDisplay.WriteHeader("Applying Migrations");
            if (!File.Exists(file))
            {
                logger.Warning("File {file} not found.", file);
                cliDisplay.Console.MarkupLineInterpolated(
                    $"[bold yellow]✗ Could not find '{file}'; database will be created.[/]");
            }

            var result = await cliDisplay.RunStatusAsync($"Applying migrations to {file}\n",
                _ =>
                {
                    try
                    {
                        try
                        {
                            var upgrader = DeployChanges.To
                                .SqliteDatabase(sqliteHelper.GetSqliteConnectionString())
                                .WithScriptsEmbeddedInAssembly(typeof(SqliteMigrator).Assembly)
                                .LogToConsole()
                                .Build();


                            var result = upgrader.PerformUpgrade();
                            if (!result.Successful)
                            {
                                cliDisplay.WriteFailure("Failed to apply migrations.");

                                return Task.FromResult(FAILED_TO_APPLY_MIGRATIONS);
                            }

                            cliDisplay.WriteSuccess("Migrations applied.");

                            return Task.FromResult(SUCCESS);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Failed to apply migrations");
                            cliDisplay.WriteFailure($"Failed to apply migrations '{e.Message}'.[/]");

                            return Task.FromResult(FAILED_TO_APPLY_MIGRATIONS);
                        }
                    }
                    catch (Exception exception)
                    {
                        return Task.FromException<int>(exception);
                    }
                }, cancellationToken);

            return result;
        }
    }
}
