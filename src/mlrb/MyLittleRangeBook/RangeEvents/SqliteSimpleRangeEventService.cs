using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Provides SQLite-specific implementation for managing SimpleRangeEvent data.
    /// </summary>
    /// <remarks>
    ///     This service offers functionalities for creating, updating, deleting, and retrieving
    ///     SimpleRangeEvent records from a SQLite database. It interacts with the database using
    ///     provided connection and transaction parameters, supporting asynchronous operations.
    /// </remarks>
    public class SqliteSimpleRangeEventService : ISimpleRangeEventService
    {
        public async Task<Result> DeleteAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent)
        {
            try
            {
                var                  p   = new { Id = simpleRangeEvent.Id! };
                DapperCommandContext ctx = context with { Arguments = p };
                int result = await Commands.s_deleteCommand
                                           .ExecuteAsync(ctx)
                                           .ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception e)
            {
                Error err = e.ToError().Enrich(simpleRangeEvent.Id!);
                return new Result().WithError(err);
            }
        }

        public async Task<Result> DisassociateFromFirearm(DapperCommandContext context, MlrbId firearmId,
                                                          MlrbId               rangeEventId)
        {
            try
            {
                var args = new { FirearmId = firearmId.ToString(), SimpleRangeEventId = rangeEventId.ToString() };
                DapperCommandContext ctx = context with { Arguments = args };

                int     l       = await Commands.s_disassociateFromFirearm.ExecuteAsync(ctx).ConfigureAwait(false);
                Success success = new($"Disassociated firearm {firearmId} with range event {rangeEventId} - {l}.");
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError());
            }
        }

        public Task<Result> ExportToCsv(DapperCommandContext context, string csvFileName) => throw new NotImplementedException();

        public async Task<Result<SimpleRangeEvent>> GetAsync(DapperCommandContext context, MlrbId simpleRangeEventId)
        {
            try
            {
                DapperCommandContext ctx = context with { Arguments = new { Id = simpleRangeEventId } };
                SimpleRangeEvent sre = await Commands.s_selectById.QuerySingleAsync<SimpleRangeEvent>(ctx)
                                                     .ConfigureAwait(false);
                return sre;
            }
            catch (Exception e)
            {
                Error err = e.ToError().Enrich(simpleRangeEventId);
                return Result.Fail(err);
            }
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(DapperCommandContext ctx)
        {
            try
            {
                IEnumerable<SimpleRangeEvent> rangeEvents = await Commands.s_selectAll
                                                                          .QueryAsync<SimpleRangeEvent>(ctx)
                                                                          .ConfigureAwait(false);

                return Result.Ok(rangeEvents);
            }
            catch (Exception e)
            {
                Error err = new($"Could not retrieve SimpleRangeEvents from database: {e.Message}");
                err.CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result<MlrbId>> UpsertAsync(DapperCommandContext context,
                                                      SimpleRangeEvent     simpleRangeEvent)
        {
            simpleRangeEvent.Modified = DateTimeOffset.UtcNow;
            if (simpleRangeEvent.RowId == null)
            {
                simpleRangeEvent.Id = MlrbId.From(simpleRangeEvent.EventDate);
            }

            try
            {
                var p = new
                        {
                            Id = simpleRangeEvent.Id!,
                            simpleRangeEvent.EventDate,
                            simpleRangeEvent.FirearmName,
                            simpleRangeEvent.RangeName,
                            simpleRangeEvent.RoundsFired,
                            simpleRangeEvent.AmmoDescription,
                            simpleRangeEvent.Notes,
                            simpleRangeEvent.Created,
                            simpleRangeEvent.Modified,
                        };
                DapperCommandContext ctx = context with { Arguments = p };
                long result = await Commands.s_upsertCommand.ExecuteScalarAsync<long>(ctx).ConfigureAwait(false);

                simpleRangeEvent.RowId = result;


                Success reason = new($"SimpleRangeEvent `{simpleRangeEvent.Id}` saved.");
                reason.Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId);
                return Result.Ok((MlrbId)simpleRangeEvent.Id!).WithSuccess(reason);
            }
            catch (Exception e)
            {
                Error err = e.ToError().Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId);

                return Result.Fail(err);
            }
        }

        static class Commands
        {
            const string UPSERT_SQL = """
                                      INSERT INTO simple_range_events (id, event_date, firearm_name, range_name, rounds_fired, ammo_description, notes, created, modified)
                                      VALUES (@Id, @EventDate, @FirearmName, @RangeName, @RoundsFired, @AmmoDescription, @Notes, @Created, @Modified)
                                      ON CONFLICT(id) DO UPDATE SET
                                        event_date = excluded.event_date,
                                        firearm_name = excluded.firearm_name,
                                        range_name = excluded.range_name,
                                        rounds_fired = excluded.rounds_fired,
                                        ammo_description = excluded.ammo_description,
                                        notes = excluded.notes,
                                        modified = excluded.modified
                                      RETURNING row_id;
                                      """;

            const string SELECT_BY_ID_SQL = """
                                            SELECT
                                                row_id AS RowId,
                                                id AS Id,
                                                event_date AS EventDate,
                                                firearm_name AS FirearmName,
                                                range_name AS RangeName,
                                                rounds_fired AS RoundsFired,
                                                ammo_description AS AmmoDescription,
                                                notes AS Notes,
                                                created AS Created,
                                                modified AS Modified
                                            FROM simple_range_events
                                            WHERE id =@Id;
                                            """;

            const string SELECT_SQL = """
                                      SELECT
                                          row_id AS RowId,
                                          id AS Id,
                                          event_date AS EventDate,
                                          firearm_name AS FirearmName,
                                          range_name AS RangeName,
                                          rounds_fired AS RoundsFired,
                                          ammo_description AS AmmoDescription,
                                          notes AS Notes,
                                          created AS Created,
                                          modified AS Modified
                                      FROM simple_range_events
                                      ORDER BY event_date, firearm_name, range_name;
                                      """;

            const string DELETE_SQL = "DELETE FROM simple_range_events WHERE id = @Id;";

            const string DISASSOCIATE_FROM_FIREARM_SQL = """
                                                         DELETE FROM firearms_simple_range_events WHERE simple_range_event_id = @SimpleRangeEventId;
                                                         """;

            internal static readonly DapperCommand s_upsertCommand           = new(UPSERT_SQL);
            internal static readonly DapperCommand s_deleteCommand           = new(DELETE_SQL);
            internal static readonly DapperCommand s_selectAll               = new(SELECT_SQL);
            internal static readonly DapperCommand s_selectById              = new(SELECT_BY_ID_SQL);
            internal static readonly DapperCommand s_disassociateFromFirearm = new(DISASSOCIATE_FROM_FIREARM_SQL);
        }
    }
}