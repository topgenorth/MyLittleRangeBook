using DbUp;
using DbUp.Engine;
using Microsoft.Extensions.Options;

namespace net.opgenorth.xero.Data.Sqlite;

public class SqliteDbZookeeper : IDbZookeeper
{
    private readonly ILogger _logger;
    private readonly FileInfo _sqliteFile;

    public SqliteDbZookeeper(ILogger logger, IOptionsSnapshot<SqliteOptions> options)
    {
        _logger = logger;
        _sqliteFile = new FileInfo(options.Value.SqliteFile);
        ConnectionString = options.Value.MakeSqliteConnectionString();
        _logger.Verbose("Connection string {connectionString}", ConnectionString);
    }

    public string ConnectionString { get; }

    public string SqliteFile => _sqliteFile.FullName;

    /// <summary>
    ///     Runs the migrations on the <b>.sqlite</b> file.
    /// </summary>
    public void UpdateDatabase()
    {
        UpgradeEngine deployer = DeployChanges.To
            .SqliteDatabase(ConnectionString)
            .LogToConsole()
            .WithScriptsEmbeddedInAssembly(GetType().Assembly)
            .Build();

        deployer.PerformUpgrade();
    }

    /// <summary>
    ///     Will delete the <b>.sqlite</b> file if it exists, create a new one, and apply migrations.
    /// </summary>
    public void CreateDatabase()
    {
        if (_sqliteFile.Exists)
        {
            _logger.Verbose("Deleting file at {FileToDelete}", _sqliteFile.FullName);
            _sqliteFile.Delete();
        }
        else
        {
            _logger.Verbose("No database exists at {FileToDelete}", _sqliteFile.FullName);
        }

        UpdateDatabase();
    }

    public override int GetHashCode() => _sqliteFile.GetHashCode();

    public override string ToString() => _sqliteFile.FullName;
}
