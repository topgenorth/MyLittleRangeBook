using System.Data;
using System.Diagnostics;
using System.Transactions;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using net.opgenorth.xero.data.sqlite;
using net.opgenorth.xero.device;
using Serilog;
using IsolationLevel = System.Data.IsolationLevel;

namespace net.opgenorth.xero.shotview
{
    public partial class MyLittleRangeBookRepository : IGetShotSession, IPersistShotSession
    {
        readonly string _connectionString;
        readonly ILogger _logger;
        readonly FileInfo _sqliteFile;

        public MyLittleRangeBookRepository(ILogger logger, IOptionsSnapshot<SqliteOptions> options)
        {
            _logger = logger;
            _sqliteFile = new FileInfo(options.Value.SqliteFile);
            _connectionString = options.Value.MakeSqliteConnectionString();
        }

        public string Filename => _sqliteFile.FullName;

    }
}
