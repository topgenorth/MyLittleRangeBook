using System.Diagnostics;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using net.opgenorth.xero.shotview;
using Serilog;

namespace net.opgenorth.xero.data.sqlite
{
    public class MyLittleRangeBookDb
    {
        readonly string _connectionString;
        readonly ILogger _logger;
        readonly FileInfo _sqliteFile;

        public MyLittleRangeBookDb(ILogger logger, IOptionsSnapshot<GarminShotViewSqliteOptions> options)
        {
            _logger = logger;
            _sqliteFile = new FileInfo(options.Value.SqliteFile);
            _connectionString = options.Value.MakeConnectionString();
        }

        public string Filename => _sqliteFile.FullName;
        public async Task UpsertSession(WorkbookSession session)
        {
            await using SqliteConnection conn = new(_connectionString);
            const string sql = "SELECT COUNT(*) AS kount FROM shotview_sessions WHERE id=@Id";
            int r = await conn.ExecuteScalarAsync<int>(sql, session);

            if (r < 1)
            {
                r = await InsertSession(conn, session);
            }
            else
            {
                r = await UpdateSession(conn, session);
            }

            Debug.Assert(r > 0, "No rows were affected.");
        }

        async Task<int> UpdateSession(SqliteConnection conn, WorkbookSession session)
        {
            const string sql = """
                               UPDATE shotview_sessions SET session_date=@SessionTimestamp, name=@SheetName, projectile_weight=@ProjectileWeight, notes=@Notes
                               WHERE id=@Id
""";

            int r = await conn.ExecuteAsync(sql, session);

            return r;
        }

        async Task<int> InsertSession(SqliteConnection connection, WorkbookSession session)
        {
            const string sql = """
                               INSERT INTO shotview_sessions (id, session_date, name, projectile_weight, notes)
                               VALUES(@Id, @SessionTimestamp, @SheetName , @ProjectileWeight, @Notes)
                               """;
            int r = await connection.ExecuteAsync(sql, session);
            _logger.Debug("finisihed");

            return r;
        }
    }
}
