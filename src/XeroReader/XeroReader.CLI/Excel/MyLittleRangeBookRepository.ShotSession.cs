using System.Data;
using System.Data.Common;
using System.Transactions;
using Dapper;
using Microsoft.Data.Sqlite;
using IsolationLevel = System.Data.IsolationLevel;

namespace net.opgenorth.xero.Excel
{
    public partial class MyLittleRangeBookRepository
    {
        public async Task<WorkbookSession> GetSessionByName(string name)
        {
            WorkbookSession? s = null;
            using (SqliteConnection conn = new(_connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                using (SqliteTransaction? trans = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    s = await LoadSessionFromSqlite(conn, trans, name);
                    await trans.RollbackAsync();
                }
            }

            return s;
        }

        public async Task<int> DeleteSession(WorkbookSession session, CancellationToken ct)
        {
            const string deleteShotsSql = "DELETE FROM shotview_shots WHERE shotview_session_id = @Id";
            const string deleteSessionSql = "DELETE FROM shotview_session WHERE Id=@Id";
            int rowDeleted = 0;
            int shotsDeleted = 0;
            var id = new { session.Id };

            using (SqliteConnection conn = new(_connectionString))
            {
                await conn.OpenAsync(ct);
                using (var trans = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                        shotsDeleted = await conn.ExecuteAsync(deleteShotsSql, id,transaction: trans);
                        rowDeleted = await conn.ExecuteAsync(deleteSessionSql, id, transaction:trans);
                        trans.CommitAsync(ct);
                }
            }

            return rowDeleted + shotsDeleted;
        }

        public async Task<int> UpsertSession(WorkbookSession session, CancellationToken ct)
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
                            shotsAffected = await UpsertShots(conn, transaction, session).ConfigureAwait(false);
                            if (shotsAffected != session.Shots.Count())
                            {
                                const string MSG_TEMPLATE =
                                    "The session {session.Id} {session.Name} has {session.ShotCount} shots, but only {affected} were affected.";
                                _logger.Warning(MSG_TEMPLATE, session.Id, session.SheetName, session.ShotCount,
                                    shotsAffected);
                            }
                        }

                        await transaction.CommitAsync(ct).ConfigureAwait(false);

                        return rowsAffected + shotsAffected;
                    }
                    catch (Exception e)
                    {
                        await transaction.RollbackAsync(ct).ConfigureAwait(false);
                        _logger.Error(e, "Could not import the sheet {sheetName}", session.SheetName);

                        return 0;
                    }
                }
            }
        }

        async Task<string?> GetSessionIdForName(SqliteConnection conn, SqliteTransaction trans, string sheetName)
        {
            const string sql = "SELECT id FROM shotview_sessions WHERE name=@SheetName";

            DbDataReader? rdr = await conn.ExecuteReaderAsync(sql, new { SheetName = sheetName }, trans);
            if (await rdr.ReadAsync())
            {
                return rdr.GetString(0);
            }

            return null;
        }

        async Task<WorkbookSession> LoadSessionFromSqlite(SqliteConnection conn, SqliteTransaction trans, string name)
        {
            const string SESSION_SQL =
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
            const string SHOTS_SQL =
                """
                SELECT id, shot_number, velocity, notes, cold_bore, clean_bore, ignore_shot, shot_time
                FROM shotview_shots WHERE id=@Id ORDER BY shot_number
                """;


            IDataReader sessionRdr =
                await conn.ExecuteReaderAsync(SESSION_SQL, new { Name = name }, trans, 1, CommandType.TableDirect);
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
                await conn.ExecuteReaderAsync(SHOTS_SQL, sessionId, trans, 1, CommandType.TableDirect);
            LoadShotsFromSqlite(shotsRdr, s);

            return s;
        }

        async Task<int> UpdateSession(SqliteConnection conn, SqliteTransaction trans, WorkbookSession session)
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


            int r = await conn.ExecuteAsync(updateSql, SessionToDynamicType(session), trans);

            return r;
        }

        async Task<int> InsertSession(SqliteConnection connection, SqliteTransaction trans, WorkbookSession session)
        {
            const string SQL = """
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

            int r = await connection.ExecuteAsync(SQL, SessionToDynamicType(session), trans);
            _logger.Verbose("Added session {session.Id} {session.SheetName} to database.", session.Id,
                session.SheetName);

            return r;
        }


        static object SessionToDynamicType(WorkbookSession session)
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
}
