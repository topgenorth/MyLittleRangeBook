using System.Data;
using System.Data.Common;
using System.Transactions;
using Dapper;
using LanguageExt;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using net.opgenorth.xero.data.sqlite;
using net.opgenorth.xero.device;
using Serilog;
using IsolationLevel = System.Data.IsolationLevel;

namespace net.opgenorth.xero.shotview;

public class MyLittleRangeBookRepository : IGetShotSession, IPersistShotSession
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


    public async Task<WorkbookSession> GetSessionByName(string name)
    {
        await using SqliteConnection conn = new(_connectionString);
        WorkbookSession s = await LoadSessionFromSqlite(conn, name);

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
            Option<string> o_id = await GetSessionIdForName(conn, session.SheetName);

            o_id.Match(
                id => session.Id = id,
                () => session.Id = string.Empty
            );

            rowsAffected = await UpdateSession(conn, session);
        }

        if (rowsAffected > 0)
        {
            await UpsertShots(conn, session);
        }
    }

    private async Task<Option<string>> GetSessionIdForName(SqliteConnection conn, string sheetName)
    {
        const string sql = "SELECT id FROM shotview_sessions WHERE name=@SheetName";

        DbDataReader rdr = await conn.ExecuteReaderAsync(sql, new { SheetName = sheetName });
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
        IDataReader sessionRdr = await conn.ExecuteReaderAsync(sessionSql, new { Name = name }, trans, 1, CommandType.TableDirect);
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
        using IDataReader shotsRdr = await conn.ExecuteReaderAsync(shotsSql, sessionId, trans, 1, CommandType.TableDirect);
        LoadShotsFromSqlite(shotsRdr, s);
        trans.Commit();

        return s;
    }

    private void LoadShotsFromSqlite(IDataReader rdr, WorkbookSession s)
    {
        while (rdr.Read())
        {
            Shot shot = new(rdr.GetString(0))
            {
                ShotNumber = rdr.GetInt32(1),
                Speed = new ShotSpeed(rdr.GetInt32(2), "fps"),
                Notes = rdr.GetString(3),
                ColdBore = rdr.GetBoolean(4),
                CleanBore = rdr.GetBoolean(5),
                IgnoreShot = rdr.GetBoolean(6),
                DateTimeUtc = rdr.GetDateTime(7)
            };
            s.AddShot(shot);
        }
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


    private static object ShotToDynamicType(Shot s, string sessionId) =>
        new
        {
            s.Id,
            SessionId = sessionId,
            s.ShotNumber,
            Velocity = s.Speed.Value,
            Notes = string.IsNullOrWhiteSpace(s.Notes) ? null : s.Notes,
            s.CleanBore,
            s.ColdBore,
            s.IgnoreShot,
            ShotTime = s.DateTimeUtc.ToString("O"),
            ModificationTime = DateTime.UtcNow.ToString("O")
        };

    private static object SessionToDynamicType(WorkbookSession session)
    {
        var values = new
        {
            DateTimeUtc = session.DateTimeUtc.ToString("O"),
            session.SheetName,
            session.ProjectileWeight,
            ProjectileType = string.IsNullOrWhiteSpace(session.ProjectileType) ? "Rifle" : session.ProjectileType,
            ProjectileUnits = string.IsNullOrWhiteSpace(session.ProjectileUnits) ? "grains" : session.ProjectileUnits,
            VelocityUnits = string.IsNullOrWhiteSpace(session.VelocityUnits) ? "fps" : session.VelocityUnits,
            Notes = string.IsNullOrWhiteSpace(session.Notes) ? null : session.Notes,
            ModificationTime = DateTime.UtcNow.ToString("O"),
            session.Id
        };


        return values;
    }

    private async Task<int> UpsertShots(SqliteConnection conn, WorkbookSession session)
    {
        if (!session.Shots.Any())
        {
            return 0;
        }

        (Shot[] shotsToInsert, Shot[] shotsToUpdate, Shot[] shotsToDelete) =
            await PutShotNumbersForSessionIntoBuckets(conn, session);

        int shotsInserted = await InsertShots(conn, session.Id, shotsToInsert);
        int shotsUpdated = await UpdateShots(conn, session.Id, shotsToUpdate);
        int shotsDeleted = await DeleteShots(conn, session.Id, shotsToDelete);

        int rowsAffected = shotsInserted + shotsDeleted + shotsUpdated;
        if (rowsAffected != session.Shots.Count())
        {
            _logger.Warning(
                "Inserted {shots.inserted}, updated {shots.updated}, and deleted {shots.deleted} shots for session {session.id}",
                shotsInserted, shotsUpdated, shotsDeleted, session.Id);
        }
        else
        {
            _logger.Verbose(
                "Inserted {shots.inserted}, updated {shots.updated}, and deleted {shots.deleted} shots for session {session.id}",
                shotsInserted, shotsUpdated, shotsDeleted, session.Id);
        }

        return rowsAffected;
    }

    private async Task<Tuple<Shot[], Shot[], Shot[]>> PutShotNumbersForSessionIntoBuckets(SqliteConnection conn, WorkbookSession session)
    {
        const string shotsSql = """
                                SELECT shot_number
                                FROM shotview_shots sh
                                LEFT JOIN shotview_sessions s
                                    ON sh.shotview_session_id=s.id
                                WHERE s.name=@SessionName
                                ORDER BY shot_number;
                                """;

        int[] existingShotNumbers = (await conn.QueryAsync<shotNumber_>(shotsSql, new { SessionName = session.SheetName }))
            .Select(si => si.shot_number)
            .ToArray();

        if (existingShotNumbers.Length == 0)
        {
            return new Tuple<Shot[], Shot[], Shot[]>(session.Shots.ToArray(), [], []);
        }


        Shot[]? allShots = session.Shots.ToArray();
        Shot[]? shotsToInsert = allShots.Where(s => existingShotNumbers.All(esi => esi != s.ShotNumber)).ToArray();
        Shot[]? shotsToUpdate = allShots.Where(s => existingShotNumbers.Any(esi => esi == s.ShotNumber)).ToArray();
        Shot[]? shotsToDelete = allShots.Where(s => shotsToInsert.Union(shotsToUpdate).All(stk => stk.Id != s.Id))
            .ToArray();

        return new Tuple<Shot[], Shot[], Shot[]>(shotsToInsert, shotsToUpdate, shotsToDelete);
    }

    private async Task<int> InsertShots(SqliteConnection conn, string sessionId, Shot[] shotsToInsert)
    {
        const string insertShot = """
                                  INSERT INTO shotview_shots (id,
                                                              shotview_session_id,
                                                              shot_number,
                                                              velocity,
                                                              notes,
                                                              cold_bore,
                                                              clean_bore,
                                                              ignore_shot,
                                                              shot_time)
                                  VALUES (@Id,
                                          @SessionId,
                                          @ShotNumber,
                                          @Velocity,
                                          @Notes,
                                          @ColdBore,
                                          @CleanBore,
                                          @IgnoreShot,
                                          @ShotTime);
                                  """;

        IEnumerable<object> inserts = shotsToInsert.Select(s => ShotToDynamicType(s, sessionId));
        if (!inserts.Any())
        {
            return 0;
        }

        int rowsInserted = await conn.ExecuteAsync(insertShot, inserts);

        return rowsInserted;
    }

    private async Task<int> UpdateShots(SqliteConnection conn, string sessionId, Shot[] shotsToUpdate)
    {
        const string updateShot = """
                                  UPDATE shotview_shots SET velocity=@Velocity,
                                                            notes=@Notes,
                                                            cold_bore=@ColdBore,
                                                            clean_bore=@CleanBore,
                                                            ignore_shot=@IgnoreShot,
                                                            shot_time=@ShotTime,
                                                            modification_date=@ModificationTime
                                  WHERE shot_number=@ShotNumber AND shotview_session_id=@SessionId;
                                  """;
        IEnumerable<object> shotUpdates = shotsToUpdate.Select(s => ShotToDynamicType(s, sessionId));
        if (!shotUpdates.Any())
        {
            return 0;
        }

        int rowsUpdated = 0;
        foreach (object update in shotUpdates)
        {
            rowsUpdated += await conn.ExecuteAsync(updateShot, update);
        }

        // int rowsUpdated = await conn.ExecuteAsync(updateShot, updates);

        return rowsUpdated;
    }

    private async Task<int> DeleteShots(SqliteConnection conn, string sessionId, Shot[] shotsToDelete)
    {
        const string deleteShot = "DELETE FROM shotview_shots WHERE shotview_session_id=@SessionId";

        IEnumerable<string> deletes = shotsToDelete.Select(s => s.Id);
        if (!deletes.Any())
        {
            return 0;
        }

        int rowsDeleted = await conn.ExecuteAsync(deleteShot, new { SessionId = sessionId });

        return rowsDeleted;
    }


    /// <summary>
    ///     This is just a helper when trying to sort out what shots are present.
    /// </summary>
    private class shotNumber_
    {
        internal int shot_number { get; set; }
    }
}
