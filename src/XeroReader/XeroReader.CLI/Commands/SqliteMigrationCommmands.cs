using MyLittleRangebook.Data.Sqlite;

namespace net.opgenorth.xero
{
    public class SqliteMigrationCommmands
    {
        readonly ILogger _logger;
        readonly SqliteDbZookeeper _sqliteDbKeeper;

        public SqliteMigrationCommmands(ILogger logger, IDbZookeeper dbz)
        {
            _logger = logger;
            _sqliteDbKeeper = (SqliteDbZookeeper) dbz;
        }

        /// <summary>
        ///     Runs the migrations on the sqlite file.
        /// </summary>
        /// <param name="directory">The directory holding the sqlite file.</param>
        public async Task<int> UpdateDatabase()
        {
            try
            {
                _sqliteDbKeeper.UpdateDatabase();
                _logger.Verbose("Updated the database {dbName}", _sqliteDbKeeper.ToString());

                return 0;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Could not update the database {dbName}.", _sqliteDbKeeper.ToString());

                return 1;
            }
        }

        /// <summary>
        ///     Will delete the sqlite file if it exists, then create a new one.
        /// </summary>
        /// <param name="directory">The directory holding the sqlite file.</param>
        public async Task<int> CreateDatabase(string directory)
        {
            try
            {
                _logger.Verbose("Create the database {dbName}", _sqliteDbKeeper.ToString());
                _sqliteDbKeeper.CreateDatabase();
                _logger.Verbose("Created database {dbName}", _sqliteDbKeeper.ToString());

                return 0;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Could not create the database {dbName}.", _sqliteDbKeeper.ToString());

                return 1;
            }
        }
    }
}
