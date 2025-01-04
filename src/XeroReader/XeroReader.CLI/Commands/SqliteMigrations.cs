using ConsoleAppFramework;
using net.opgenorth.xero.data.sqlite;

namespace net.opgenorth.xero.Commands;

public class SqliteMigrations
{
    private readonly ILogger _logger;
    private readonly IDbZookeeper _sqliteDbKeeper;

    public SqliteMigrations(ILogger logger, IDbZookeeper dbz)
    {
        _logger = logger;
        _sqliteDbKeeper = (SqliteDbZookeeper)dbz;
    }


    /// <summary>
    ///     Runs the migrations on the sqlite file.
    /// </summary>
    [Command("upgrade")]
    public Task<int> UpdateDatabase()
    {
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
    public async Task<int> CreateDatabase()
    {
        try
        {
            _sqliteDbKeeper.CreateDatabase();
            _logger.Information("Created database {dbName}", _sqliteDbKeeper.ToString());
            return 0;
        }
        catch (Exception e)
        {
            _logger.Fatal(e, "Could not create the database {dbName}.", _sqliteDbKeeper.ToString());

            return 1;
        }
    }
}
