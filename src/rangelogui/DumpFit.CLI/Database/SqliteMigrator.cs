using ConsoleAppFramework;
using DbUp;
using Microsoft.Data.Sqlite;
using MySimpleRangeLog.CLI;
using Spectre.Console;
using SQLitePCL;

namespace MySimpleRangeLog.Database
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
        /// Ensures that all database schema migrations are applied to the specified SQLite database file.
        /// </summary>
        /// <param name="file">The full path to the SQLite database file.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("migrate")]
        public async Task<int> MigrateSchemaAsync(string file, CancellationToken ct)
        {

// At app startup, e.g. in your shell or Program.cs
            raw.SetProvider(new SQLite3Provider_e_sqlite3());
// or, if you’re using bundle_e_sqlite3:
            Batteries.Init();

            if (!File.Exists(file))
            {
                _logger.Warning("File {file} not found.", file);
                _console.MarkupLineInterpolated($"[bold yellow]✗ [/] Could not find '{file}'; database will be created.");
            }

            try
            {
                var upgrader = DeployChanges.To
                    .SqliteDatabase(BuildConnectionString(file))
                    .WithScriptsEmbeddedInAssembly(typeof(SqliteMigrator).Assembly)
                    .LogToConsole()
                    .Build();

                if (upgrader.IsUpgradeRequired())
                {
                    _console.MarkupLine("[Green]Migrations will be applied.[/]");
                }
                else
                {
                    _console.MarkupLine("[yellow]✓ No upgrade is required.[/]");
                }

                var result = upgrader.PerformUpgrade();
                if (!result.Successful)
                {
                    _console.MarkupLineInterpolated(
                        $"[bold red]✗ Error:[/] failed to apply migrations '{result.Error}'");

                    return ReturnCodes.FAILED_TO_APPLY_MIGRATIONS;
                }

                _console.MarkupLine("[green]✓ Finished.[/]");

                return ReturnCodes.SUCCESS;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to apply migrations");
                _console.MarkupLineInterpolated($"[bold red]✗ Error:[/] failed to apply migrations '{e.Message}'");

                return ReturnCodes.FAILED_TO_APPLY_MIGRATIONS;
            }
        }
    }
}
