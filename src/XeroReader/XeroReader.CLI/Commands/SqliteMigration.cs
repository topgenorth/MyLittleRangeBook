using net.opgenorth.mylittlerangebook.data.sqlite;

namespace net.opgenorth.xero
{
    public class SqliteMigration
    {
        readonly ILogger _logger;
        readonly SqliteDbZookeeper _sqliteDbKeeper;

        public SqliteMigration(ILogger logger, IDbZookeeper dbz)
        {
            _logger = logger;
            _sqliteDbKeeper = (SqliteDbZookeeper) dbz;
        }

        /// <summary>
        ///     Runs the migrations on the sqlite file.
        /// </summary>
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
        public async Task<int> CreateDatabase()
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
