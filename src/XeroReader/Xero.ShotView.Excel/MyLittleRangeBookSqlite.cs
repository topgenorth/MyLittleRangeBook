using System.Diagnostics;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using net.opgenorth.xero.device;
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

        public async Task<int> DeleteSession(WorkbookSession session)
        {
            await using SqliteConnection conn = new(_connectionString);
            int rowDeleted =
                await conn.ExecuteAsync("DELETE FROM shotview_session WHERE Id=@Id", new { session.Id });

            return rowDeleted;
        }

        public async Task UpsertSession(WorkbookSession session)
        {
            await using SqliteConnection conn = new(_connectionString);
            const string sql = "SELECT COUNT(*) AS kount FROM shotview_sessions WHERE id=@Id";
            int rowsAffected = await conn.ExecuteScalarAsync<int>(sql, session);

            if (rowsAffected < 1)
            {
                rowsAffected = await InsertSession(conn, session);
            }
            else
            {
                rowsAffected = await UpdateSession(conn, session);
            }

            if (rowsAffected > 0)
            {
                UpsertShots(conn, session);
            }

            Debug.Assert(rowsAffected > 0, "No rows were affected.");
        }

        async Task<int> UpdateSession(SqliteConnection conn, WorkbookSession session)
        {
            const string updateSql = """
                                     UPDATE shotview_sessions
                                     SET session_date=@SessionTimestamp,
                                         name=@SheetName,
                                         projectile_weight=@ProjectileWeight,
                                         notes=@Notes,
                                         modification_time=@ModificationTime,
                                         projectile_units=@ProjectileUnits,
                                         velocity_units=@VelocityUnits
                                     WHERE id=@Id
                                     """;

            var update = new
            {
                session.Id,
                SessionTimestamp = session.SessionTimestamp.ToString("O"),
                Name = session.SheetName,
                session.ProjectileWeight,
                session.Notes,
                ModificationTime = DateTime.UtcNow.ToString("O"),
                ProjectileUnits = "grains",
                VelocityUnits = "grains"
            };
            int r = await conn.ExecuteAsync(updateSql, update);

            return r;
        }

        async Task<int> InsertSession(SqliteConnection connection, WorkbookSession session)
        {
            const string sql = """
                               INSERT INTO shotview_sessions (id, session_date, name, projectile_weight, notes)
                               VALUES(@Id, @SessionTimestamp, @SheetName , @ProjectileWeight, @Notes)
                               """;
            int r = await connection.ExecuteAsync(sql, session);
            _logger.Verbose("Added session {session.id} to database.", session.Id);

            return r;
        }

        async Task<int> UpsertShots(SqliteConnection conn, WorkbookSession session)
        {
            if (!session.Shots.Any())
            {
                return 0;
            }

            (Shot[] shotsToInsert, Shot[] shotsToUpdate, Shot[] shotsToDelete) =
                await PutShotsIntoBucket(conn, session);

            int shotsInserted = await InsertShots(conn, session.Id, shotsToInsert);
            int shotsUpdated = await UpdateShots(conn, shotsToUpdate);
            int shotsDeleted = await DeleteShots(conn, shotsToDelete);

            _logger.Verbose(
                "Inserted {shots.inserted}, updated {shots.updated}, and deleted {shots.deleted} shots for session {session.id}",
                shotsInserted, shotsUpdated, shotsDeleted, session.Id);

            int rowsAffected = shotsInserted + shotsDeleted + shotsUpdated;

            return rowsAffected;
        }

        async Task<Tuple<Shot[], Shot[], Shot[]>> PutShotsIntoBucket(SqliteConnection conn, WorkbookSession session)
        {
            const string shotsSql = "SELECT id FROM shotview_shots WHERE shotview_session_id=@Id";
            string[] existingShotIds = (await conn.QueryAsync<shotId>(shotsSql, new { session.Id }))
                .Select(si => si.id)
                .ToArray();

            if (existingShotIds.Length == 0)
            {
                return new Tuple<Shot[], Shot[], Shot[]>(session.Shots.ToArray(), [], []);
            }

            Shot[]? allShots = session.Shots.ToArray();

            Shot[]? shotsToInsert = allShots.Where(s => existingShotIds.All(esi => esi != s.Id)).ToArray();
            Shot[]? shotsToUpdate = allShots.Where(s => existingShotIds.Any(esi => esi == s.Id)).ToArray();
            Shot[]? shotsToDelete = allShots.Where(s => shotsToInsert.Union(shotsToUpdate).All(stk => stk.Id != s.Id))
                .ToArray();

            return new Tuple<Shot[], Shot[], Shot[]>(shotsToInsert, shotsToUpdate, shotsToDelete);
        }

        async Task<int> InsertShots(SqliteConnection conn, string sessionId, Shot[] shotsToInsert)
        {
            const string insertShot = """
                                      INSERT INTO shotview_shots (id, shotview_session_id, shot_number, velocity, notes, cold_bore, clean_bore, ignore_shot, shot_time)
                                      VALUES (@Id, @SessionId, @ShotNumber, @Velocity, @Notes, @ColdBore, @CleanBore, @IgnoreShot, @ShotTime);
                                      """;

            var inserts = shotsToInsert.Select(s => new
            {
                s.Id,
                SessionId = sessionId,
                s.ShotNumber,
                Velocity = s.Speed.Value,
                s.Notes,
                s.CleanBore,
                s.ColdBore,
                s.IgnoreShot,
                ShotTime = s.Timestamp.ToUniversalTime().ToString("O")
            });

            if (!inserts.Any())
            {
                return 0;
            }

            int rowsInserted = await conn.ExecuteAsync(insertShot, inserts);

            return rowsInserted;
        }

        async Task<int> UpdateShots(SqliteConnection conn, Shot[] shotsToUpdate)
        {
            const string updateShot = """
                                      UPDATE shotview_shots SET velocity=@Velocity, notes=@Notes,
                                                                cold_bore=@ColdBore, clean_bore=@CleanBore,
                                                                ignore_shot=@IgnoreShot,
                                                                shot_time=@ShotTime,
                                                                modification_time=@ModificationTime
                                      WHERE (id=@Id);
                                      """;
            var updates = shotsToUpdate.Select(s => new
            {
                s.Id,
                s.ShotNumber,
                Velocity = s.Speed.Value,
                s.Notes,
                s.ColdBore,
                s.CleanBore,
                s.IgnoreShot,
                ShotTime = s.Timestamp.ToUniversalTime().ToString("O"),
                ModificationTime = DateTime.UtcNow.ToString("O")
            });
            if (!updates.Any())
            {
                return 0;
            }

            int rowsUpdated = await conn.ExecuteAsync(updateShot, updates);

            return rowsUpdated;
        }

        async Task<int> DeleteShots(SqliteConnection conn, Shot[] shotsToDelete)
        {
            const string deleteShot = "DELETE FROM shotview_shots WHERE id=@Id";

            IEnumerable<string> deletes = shotsToDelete.Select(s => s.Id);
            if (!deletes.Any())
            {
                return 0;
            }

            int rowsDeleted = await conn.ExecuteAsync(deleteShot, shotsToDelete);

            return rowsDeleted;
        }


        class shotId
        {
            internal string id { get; set; }
        }
    }
}
