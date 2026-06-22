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
                var p = new { Id = simpleRangeEvent.Id! };
                int result = await Commands.DeleteCommand
                                           .ExecuteAsync(context)
                                           .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Error err = new($"Could not delete SimpleRangeEvent `{simpleRangeEvent.Id}`: {e.Message}");
                err.CausedBy(e).Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId);

                return Result.Fail(err);
            }

            return Result.Ok();
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(DapperCommandContext ctx)
        {
            try
            {
                IEnumerable<SimpleRangeEvent> rangeEvents = await Commands.SelectAll
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

        public async Task<Result<long?>> UpsertAsync(DapperCommandContext context,
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
                long result = await Commands.UpsertCommand.ExecuteScalarAsync<long>(ctx).ConfigureAwait(false);

                simpleRangeEvent.RowId = result;

                Success reason = new($"SimpleRangeEvent `{simpleRangeEvent.Id}` saved.");
                reason.WithMetadata("Id",    simpleRangeEvent.Id);
                reason.WithMetadata("RowId", simpleRangeEvent.RowId);


                return Result.Ok(simpleRangeEvent.RowId).WithSuccess(reason);
            }
            catch (Exception e)
            {
                Error err = new($"Could not save SimpleRangeEvent `{simpleRangeEvent.Id}`: {e.Message}");
                err.CausedBy(e).Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId);

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

            const         string        DELETE_SQL = "DELETE FROM simple_range_events WHERE id = @Id;";
            public static DapperCommand UpsertCommand => new(UPSERT_SQL);
            public static DapperCommand DeleteCommand => new(DELETE_SQL);
            public static DapperCommand SelectAll     => new(SELECT_SQL);
        }
    }
}