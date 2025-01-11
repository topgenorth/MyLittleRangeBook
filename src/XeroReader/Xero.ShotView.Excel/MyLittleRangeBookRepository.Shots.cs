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

        async Task<int> UpsertShots(SqliteConnection conn, SqliteTransaction trans, WorkbookSession session)
        {
            if (!session.Shots.Any())
            {
                return 0;
            }

            (Shot[] shotsToInsert, Shot[] shotsToUpdate, Shot[] shotsToDelete) =
                await PutShotNumbersForSessionIntoBuckets(conn, session);

            int shotsInserted = await InsertShots(conn, trans, session.Id, shotsToInsert).ConfigureAwait(false);
            int shotsUpdated = await UpdateShots(conn, trans, session.Id, shotsToUpdate).ConfigureAwait(false);
            int shotsDeleted = await DeleteShots(conn, trans, session.Id, shotsToDelete).ConfigureAwait(false);

            int rowsAffected = shotsInserted + shotsDeleted + shotsUpdated;

            return rowsAffected;
        }

        async Task<Tuple<Shot[], Shot[], Shot[]>> PutShotNumbersForSessionIntoBuckets(SqliteConnection conn,
            WorkbookSession session)
        {
            const string SHOTS_SQL = """
                                     SELECT shot_number
                                     FROM shotview_shots sh
                                     LEFT JOIN shotview_sessions s
                                         ON sh.shotview_session_id=s.id
                                     WHERE s.name=@SessionName
                                     ORDER BY shot_number;
                                     """;

            int[] existingShotNumbers =
                (await conn.QueryAsync<shotNumber_>(SHOTS_SQL, new { SessionName = session.SheetName }))
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

        async Task<int> InsertShots(SqliteConnection conn,
            SqliteTransaction trans,
            string sessionId,
            Shot[] shotsToInsert)
        {
            const string INSERT_SHOT = """
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

            object[] inserts = shotsToInsert.Select(s => ShotToDynamicType(s, sessionId)).ToArray();
            if (inserts.Length == 0)
            {
                return 0;
            }


            int rowsInserted = await conn.ExecuteAsync(INSERT_SHOT, inserts, trans).ConfigureAwait(false);

            return rowsInserted;
        }

        async Task<int> UpdateShots(SqliteConnection conn,
            SqliteTransaction trans,
            string sessionId,
            Shot[] shotsToUpdate)
        {
            const string UPDATE_SHOT = """
                                       UPDATE shotview_shots SET velocity=@Velocity,
                                                                 notes=@Notes,
                                                                 cold_bore=@ColdBore,
                                                                 clean_bore=@CleanBore,
                                                                 ignore_shot=@IgnoreShot,
                                                                 shot_time=@ShotTime,
                                                                 modification_date=@ModificationTime
                                       WHERE shot_number=@ShotNumber AND shotview_session_id=@SessionId;
                                       """;
            object[] shotUpdates = shotsToUpdate.Select(s => ShotToDynamicType(s, sessionId)).ToArray();
            if (shotUpdates.Length == 0)
            {
                return 0;
            }

            int rowsUpdated = 0;
            foreach (object update in shotUpdates)
            {
                try
                {
                    rowsUpdated += await conn.ExecuteAsync(UPDATE_SHOT, update, trans).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not update shot");
                }
            }


            return rowsUpdated;
        }

        async Task<int> DeleteShots(SqliteConnection conn,
            SqliteTransaction trans,
            string sessionId,
            Shot[] shotsToDelete)
        {
            const string DELETE_SHOT = "DELETE FROM shotview_shots WHERE shotview_session_id=@SessionId";

            IEnumerable<string> deletes = shotsToDelete.Select(s => s.Id);
            if (!deletes.Any())
            {
                return 0;
            }

            int rowsDeleted = await conn.ExecuteAsync(DELETE_SHOT, new { SessionId = sessionId }, trans)
                .ConfigureAwait(false);

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
