using System.Diagnostics;
using ConsoleAppFramework;
using net.opgenorth.xero.data.sqlite;

namespace net.opgenorth.xero.Commands
{
    public class SqliteMigrations
    {
        readonly ILogger _logger;
        readonly IDbZookeeper _sqliteDbKeeper;

        public SqliteMigrations(ILogger logger, IDbZookeeper dbz)
        {
            _logger = logger;
            _sqliteDbKeeper = (SqliteDbZookeeper)dbz;
        }

        public static string GetAppNameAndVersion()
        {
            string[] args = Environment.GetCommandLineArgs();
            string location = Environment.GetCommandLineArgs()[0];
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(location);

            return fvi is null ? location : $"{fvi.ProductName} {fvi.ProductVersion}";
        }

        /// <summary>
        ///     Runs the migrations on the sqlite file.
        /// </summary>
        [Command("upgrade")]
        public Task<int> UpdateDatabase()
        {
            _logger.Information("{appName}", GetAppNameAndVersion());

            try
            {
                _sqliteDbKeeper.UpdateDatabase();
                _logger.Information("Updated the database {dbName}", _sqliteDbKeeper.ToString());

                return Task.FromResult(0);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Could not update the database {dbName}.", _sqliteDbKeeper.ToString());

                return Task.FromResult(1);
            }
        }

        /// <summary>
        ///     Will delete the sqlite file if it exists, then create a new one.
        /// </summary>
        [Command("init")]
        // ReSharper disable once UnusedMember.Global
        public Task<int> CreateDatabase()
        {
            _logger.Information("{appName}", GetAppNameAndVersion());

            try
            {
                _sqliteDbKeeper.CreateDatabase();
                _logger.Information("Created database {dbName}", _sqliteDbKeeper.ToString());

                return Task.FromResult(0);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Could not create the database {dbName}.", _sqliteDbKeeper.ToString());

                return Task.FromResult(1);
            }
        }
    }
}
