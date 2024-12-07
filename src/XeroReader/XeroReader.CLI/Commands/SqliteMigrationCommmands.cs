
using MyLittleRangebook.Data.Sqlite;

namespace net.opgenorth.xero
{
    public class SqliteMigrationCommmands
    {
        readonly ILogger _logger;

        public SqliteMigrationCommmands(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Runs the migrations on the <b>.sqlite</b> file.
        /// </summary>
        public async Task<int> UpdateDatabase(string directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            string? dbName = string.Empty;
            try
            {
                var dbz = new DbZookeeper(_logger, directory);
                dbName = dbz.SqliteFile;
                dbz.UpdateDatabase();

                _logger.Verbose("Updated the database {dbName}", dbName);
                return 0;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Could not update the database {dbName}.", dbName);
                return 1;
            }
        }

        /// <summary>
        /// Will delete the <b>.sqlite</b> file if it exists, then create a new one.
        /// </summary>
        public async Task<int> CreateDatabase(string directory)
        {
            var dbName = string.Empty;
            try
            {
                var dbz = new DbZookeeper(_logger, directory);
                dbName = dbz.SqliteFile;
                dbz.CreateDatabase();
                _logger.Verbose("Created database {dbName}", dbName);
                return 0;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Could not create the database {dbName}.", dbName);
                return 1;
            }
        }
    }
}
