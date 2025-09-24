using Microsoft.Extensions.Options;
using net.opgenorth.xero.Data.Sqlite;

namespace net.opgenorth.xero.Excel
{
    public partial class MyLittleRangeBookRepository : IGetShotSession, IPersistShotSession
    {
        readonly string _connectionString;
        readonly ILogger _logger;
        readonly FileInfo _sqliteFile;

        public MyLittleRangeBookRepository(ILogger logger, IOptionsSnapshot<SqliteOptions> options)
        {
            _logger = logger;
            SqliteOptions? o = options.Value.InferDataDirectory();
            _sqliteFile = new FileInfo(o.SqliteFile);
            _connectionString = o.MakeSqliteConnectionString();
        }

        public string Filename => _sqliteFile.FullName;
    }
}
