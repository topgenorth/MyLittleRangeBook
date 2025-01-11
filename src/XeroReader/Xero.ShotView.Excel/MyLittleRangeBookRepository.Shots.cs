
using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using net.opgenorth.xero.device;

namespace net.opgenorth.xero.shotview
{
    public partial class MyLittleRangeBookRepository
    {
        void LoadShotsFromSqlite(IDataReader rdr, WorkbookSession s)
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

        static object ShotToDynamicType(Shot s, string sessionId) => new
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

        async Task<int> UpsertShots(SqliteConnection conn, WorkbookSession session)
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
                    "Inserted {shots.inserted}, updated {shots.updated}, and deleted {shots.deleted} shots for session {session.id} {session.sheetName}",
                    shotsInserted, shotsUpdated, shotsDeleted, session.Id, session.SheetName);
            }
            else
            {
                _logger.Verbose(
                    "Inserted {shots.inserted}, updated {shots.updated}, and deleted {shots.deleted} shots for session {session.id}",
                    shotsInserted, shotsUpdated, shotsDeleted, session.Id);
            }

            return rowsAffected;
        }

        async Task<Tuple<Shot[], Shot[], Shot[]>> PutShotNumbersForSessionIntoBuckets(SqliteConnection conn,
            WorkbookSession session)
        {
            const string shotsSql = """
                                    SELECT shot_number
                                    FROM shotview_shots sh
                                    LEFT JOIN shotview_sessions s
                                        ON sh.shotview_session_id=s.id
                                    WHERE s.name=@SessionName
                                    ORDER BY shot_number;
                                    """;

            int[] existingShotNumbers =
                (await conn.QueryAsync<shotNumber_>(shotsSql, new { SessionName = session.SheetName }))
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

        async Task<int> InsertShots(SqliteConnection conn, string sessionId, Shot[] shotsToInsert)
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

        async Task<int> UpdateShots(SqliteConnection conn, string sessionId, Shot[] shotsToUpdate)
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

        async Task<int> DeleteShots(SqliteConnection conn, string sessionId, Shot[] shotsToDelete)
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
        class shotNumber_
        {
            internal int shot_number { get; set; }
        }
    }
}
