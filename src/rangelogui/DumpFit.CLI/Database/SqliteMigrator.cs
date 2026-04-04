using ConsoleAppFramework;
using DbUp;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace MySimpleRangeLog.CLI.Database
{
    [RegisterCommands("schema")]
    public class SqliteMigrator
    {
        readonly IAnsiConsole _console;
        readonly ILogger _logger;

        public SqliteMigrator(ILogger logger, IAnsiConsole? console)
        {
            _logger = logger;
            _console = console ?? AnsiConsole.Console;
        }


        /// <summary>
        /// Creates a connection string for the specified SQLite database file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal string BuildConnectionString(string filename)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = filename,
                Mode = SqliteOpenMode.ReadWriteCreate,
            };

            return builder.ToString();
        }

        /// <summary>
        /// Will return all the migrations that have been applied to the database.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("version")]
        public async Task<int> MigrationVersionAsync(string file, CancellationToken ct)
        {
            if (!File.Exists(file))
            {
                _logger.Warning("File {file} not found.", file);
                _console.MarkupLineInterpolated($"[bold yellow]✗ Could not find '{file}'.[/]");
                return ReturnCodes.DATABASE_FILE_NOT_FOUND;
            }
            
            _console.MarkupLineInterpolated($"[bold green]✓ Checking migration version for '{file}'.[/]");
            
            using var connection = new SqliteConnection(BuildConnectionString(file));
            await connection.OpenAsync(ct);
            
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM SchemaVersions ORDER BY Applied";
            await using var rdr = await cmd.ExecuteReaderAsync(ct);
            
            var table = new Table();
            table.AddColumn("ID");
            table.AddColumn("Script Name");
            table.AddColumn("Date Applied");

            while (await rdr.ReadAsync(ct))
            {
                string schemaVersionId = (rdr.IsDBNull(0) ? string.Empty : rdr.GetValue(0).ToString()) ?? string.Empty;
                string scriptName = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                string applied = rdr.IsDBNull(2) ? string.Empty : rdr.GetValue(2).ToString()?? string.Empty;

                table.AddRow(schemaVersionId!, scriptName, applied!);
            }
            
            _console.Write(table);
            return ReturnCodes.SUCCESS;
        }

        /// <summary>
        /// Run the SQL statements to insert data into the database.
        /// </summary>
        /// <param name="file">The database file.</param>
        /// <param name="sqlfile">The SQL file to use.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("sql")]
        public async Task<int> RunSqlOnDatabase(string file, string sqlfile, CancellationToken ct)
        {
            if (!File.Exists(file))
            {
                _logger.Warning("File {file} not found.", file);
                _console.MarkupLineInterpolated($"[bold red]✗ Could not find the database '{file}'.[/]");
                return ReturnCodes.DATABASE_FILE_NOT_FOUND;
            }

            if (!File.Exists(sqlfile))
            {
                _logger.Warning("SQL File {sqlFile} not found.", sqlfile);
                _console.MarkupLineInterpolated($"[bold red]✗ Could not find the SQL '{sqlfile}'.[/]");
                return ReturnCodes.SQL_FILE_NOT_FOUND;
            }
            try
            {
                var sql = await File.ReadAllTextAsync(sqlfile, ct);
                using var connection = new SqliteConnection(BuildConnectionString(file));
                await connection.OpenAsync(ct);

                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync(ct);
                return ReturnCodes.SUCCESS;

            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to run SQL.");
                _console.MarkupLineInterpolated($"[bold red]✗ Error: failed to run SQL '{e.Message}'.[/]");
                return ReturnCodes.FAILED_TO_RUN_SQL;
            }
            
            return ReturnCodes.SUCCESS;
        }
        /// <summary>
        /// Ensures that all database schema migrations are applied to the specified SQLite database file.
        /// </summary>
        /// <param name="file">The full path to the SQLite database file.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("migrate")]
        public async Task<int> MigrateSchemaAsync(string file, CancellationToken ct)
        {
            if (!File.Exists(file))
            {
                _logger.Warning("File {file} not found.", file);
                _console.MarkupLineInterpolated($"[bold yellow]✗ Could not find '{file}'; database will be created.[/]");
            }

            try
            {
                var upgrader = DeployChanges.To
                    .SqliteDatabase(BuildConnectionString(file))
                    .WithScriptsEmbeddedInAssembly(typeof(SqliteMigrator).Assembly)
                    .LogToConsole()
                    .Build();

                _console.MarkupLine(upgrader.IsUpgradeRequired()
                    ? "[Green]Migrations will be applied.[/]"
                    : "[yellow]✓ No upgrade is required.[/]");

                var result = upgrader.PerformUpgrade();
                if (!result.Successful)
                {
                    _console.MarkupLineInterpolated(
                        $"[bold red]✗ Error: failed to apply migrations '{result.Error}'.[/]");

                    return ReturnCodes.FAILED_TO_APPLY_MIGRATIONS;
                }

                _console.MarkupLine("[green]✓ Finished.[/]");

                return ReturnCodes.SUCCESS;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to apply migrations");
                _console.MarkupLineInterpolated($"[bold red]✗ Error: failed to apply migrations '{e.Message}'.[/]");

                return ReturnCodes.FAILED_TO_APPLY_MIGRATIONS;
            }
        }
    }
}
