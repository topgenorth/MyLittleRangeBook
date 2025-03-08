using Microsoft.Extensions.Options;
using net.opgenorth.xero.Data.Sqlite;

namespace net.opgenorth.xero.ShotView.Excel;

public partial class MyLittleRangeBookRepository : IGetShotSession, IPersistShotSession
{
    private readonly string _connectionString;
    private readonly ILogger _logger;
    private readonly FileInfo _sqliteFile;

    public MyLittleRangeBookRepository(ILogger logger, IOptionsSnapshot<SqliteOptions> options)
    {
        _logger = logger;
        _sqliteFile = new FileInfo(options.Value.SqliteFile);
        _connectionString = options.Value.MakeSqliteConnectionString();
    }

    public string Filename => _sqliteFile.FullName;
}
