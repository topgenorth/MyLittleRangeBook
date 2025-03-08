using System.Data;
using System.Data.Common;
using System.Transactions;
using Dapper;
using Microsoft.Data.Sqlite;
using IsolationLevel = System.Data.IsolationLevel;

namespace net.opgenorth.xero.ShotView.Excel;

public partial class MyLittleRangeBookRepository
{
    public async Task<WorkbookSession> GetSessionByName(string name)
    {
        await using SqliteConnection conn = new(_connectionString);
        WorkbookSession? s = await LoadSessionFromSqlite(conn, name);

        return s;
    }

    public async Task<int> DeleteSession(WorkbookSession session)
    {
        const string deleteShotsSql = "DELETE FROM shotview_shots WHERE shotview_session_id = @Id";
        const string deleteSessionSql = "DELETE FROM shotview_session WHERE Id=@Id";
        int rowDeleted = 0;
        int shotsDeleted = 0;
        var id = new { session.Id };

        await using SqliteConnection conn = new(_connectionString);
        using (TransactionScope scope = new())
        {
            shotsDeleted = await conn.ExecuteAsync(deleteShotsSql, id);
            rowDeleted = await conn.ExecuteAsync(deleteSessionSql, id);
            scope.Complete();
        }

        return rowDeleted + shotsDeleted;
    }

    public async Task UpsertSession(WorkbookSession session)
    {
        await using SqliteConnection conn = new(_connectionString);
        const string sql = "SELECT COUNT(*) AS kount FROM shotview_sessions WHERE name=@SheetName";
        int rowsAffected = await conn.ExecuteScalarAsync<int>(sql, new { session.SheetName });

        if (rowsAffected < 1)
        {
            rowsAffected = await InsertSession(conn, session);
        }
        else
        {
            session.Id = await GetSessionIdForName(conn, session.SheetName) ??
                         throw new InvalidOperationException("Could not find a session named " + session.SheetName);
            rowsAffected = await UpdateSession(conn, session);
        }

        if (rowsAffected > 0)
        {
            await UpsertShots(conn, session);
        }
    }

    private async Task<string?> GetSessionIdForName(SqliteConnection conn, string sheetName)
    {
        const string sql = "SELECT id FROM shotview_sessions WHERE name=@SheetName";

        DbDataReader? rdr = await conn.ExecuteReaderAsync(sql, new { SheetName = sheetName });
        if (await rdr.ReadAsync())
        {
            return rdr.GetString(0);
        }

        return null;
    }

    private async Task<WorkbookSession> LoadSessionFromSqlite(SqliteConnection conn, string name)
    {
        const string sessionSql =
            """
            SELECT id,
                   session_date,
                   name,
                   projectile_type,
                   projectile_weight,
                   projectile_units,
                   velocity_units,
                   notes
            FROM shotview_sessions
            WHERE name=@Name
            """;
        const string shotsSql =
            """
            SELECT id, shot_number, velocity, notes, cold_bore, clean_bore, ignore_shot, shot_time
            FROM shotview_shots WHERE id=@Id ORDER BY shot_number
            """;


        await using SqliteTransaction? trans = conn.BeginTransaction(IsolationLevel.Snapshot);
        IDataReader sessionRdr =
            await conn.ExecuteReaderAsync(sessionSql, new { Name = name }, trans, 1, CommandType.TableDirect);
        sessionRdr.Read();
        WorkbookSession s = new() { SheetName = name };
        s.DateTimeUtc = sessionRdr.GetDateTime(1);
        s.SheetName = sessionRdr.GetString(2);
        s.ProjectileType = sessionRdr.GetString(3);
        s.ProjectileWeight = sessionRdr.GetInt32(4);
        s.ProjectileUnits = sessionRdr.GetString(5);
        s.VelocityUnits = sessionRdr.GetString(6);
        s.Notes = sessionRdr.GetString(7);
        sessionRdr.Dispose();

        var sessionId = new { s.Id };
        using IDataReader shotsRdr =
            await conn.ExecuteReaderAsync(shotsSql, sessionId, trans, 1, CommandType.TableDirect);
        LoadShotsFromSqlite(shotsRdr, s);
        trans.Commit();

        return s;
    }

    private async Task<int> UpdateSession(SqliteConnection conn, WorkbookSession session)
    {
        const string updateSql = """
                                 UPDATE shotview_sessions
                                 SET session_date=@DateTimeUtc,
                                     projectile_weight=@ProjectileWeight,
                                     projectile_type=@ProjectileType,
                                     projectile_units=@ProjectileUnits,
                                     velocity_units=@VelocityUnits,
                                     notes=@Notes,
                                     modification_date=@ModificationTime
                                 WHERE name=@SheetName
                                 """;


        int r = await conn.ExecuteAsync(updateSql, SessionToDynamicType(session));

        return r;
    }

    private async Task<int> InsertSession(SqliteConnection connection, WorkbookSession session)
    {
        const string sql = """
                           INSERT INTO shotview_sessions (id,
                                                          session_date,
                                                          name,
                                                          projectile_weight,
                                                          projectile_type,
                                                          projectile_units,
                                                          velocity_units,
                                                          notes,
                                                          modification_date)
                           VALUES(@Id,
                                  @DateTimeUtc,
                                  @SheetName ,
                                  @ProjectileWeight,
                                  @ProjectileType,
                                  @ProjectileUnits,
                                  @VelocityUnits,
                                  @Notes,
                                  @ModificationTime)
                           """;

        int r = await connection.ExecuteAsync(sql, SessionToDynamicType(session));
        _logger.Verbose("Added session {session.id} to database.", session.Id);

        return r;
    }


    private static object SessionToDynamicType(WorkbookSession session)
    {
        var values = new
        {
            DateTimeUtc = session.DateTimeUtc.ToString("O"),
            session.SheetName,
            session.ProjectileWeight,
            ProjectileType =
                string.IsNullOrWhiteSpace(session.ProjectileType) ? "Rifle" : session.ProjectileType,
            ProjectileUnits =
                string.IsNullOrWhiteSpace(session.ProjectileUnits) ? "grains" : session.ProjectileUnits,
            VelocityUnits = string.IsNullOrWhiteSpace(session.VelocityUnits) ? "fps" : session.VelocityUnits,
            Notes = string.IsNullOrWhiteSpace(session.Notes) ? null : session.Notes,
            ModificationTime = DateTime.UtcNow.ToString("O"),
            session.Id
        };


        return values;
    }
}
