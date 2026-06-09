using System.Data;
using Dapper;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    /// Provides SQLite-specific implementation for managing SimpleRangeEvent data.
    /// </summary>
    /// <remarks>
    /// This service offers functionalities for creating, updating, deleting, and retrieving
    /// SimpleRangeEvent records from a SQLite database. It interacts with the database using
    /// provided connection and transaction parameters, supporting asynchronous operations.
    /// </remarks>
    public class SqliteSimpleRangeEventService : ISimpleRangeEventService
    {
        const string UpsertSql = """
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

        const string SelectSql = """
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

        const string DeleteSql = "DELETE FROM simple_range_events WHERE id = @Id;";

        static DapperCommand UpsertCommand => new(UpsertSql);
        static DapperCommand DeleteCommand => new(DeleteSql);
        static DapperCommand SelectAll => new(SelectSql, new { });

        public async Task<Result> DeleteAsync(IDbConnection connection,
            SimpleRangeEvent simpleRangeEvent,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var p = new { Id = simpleRangeEvent.Id! };
                DapperCommandContext ctx = new DapperCommandContext(connection, transaction, cancellationToken, p);
                int result = await DeleteCommand
                    .ExecuteAsync(ctx)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not delete SimpleRangeEvent `{simpleRangeEvent.Id}`: {e.Message}");
                err.CausedBy(e).Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId);

                return Result.Fail(err);
            }

            return Result.Ok();
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var ctx = new DapperCommandContext(connection, transaction, cancellationToken);
                IEnumerable<SimpleRangeEvent> rangeEvents = await SelectAll
                    .QueryAsync<SimpleRangeEvent>(ctx)
                    .ConfigureAwait(false);

                return Result.Ok(rangeEvents);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not retrieve SimpleRangeEvents from database: {e.Message}");
                err.CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result<long?>> UpsertAsync(IDbConnection connection,
            SimpleRangeEvent simpleRangeEvent,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            simpleRangeEvent.Modified = DateTimeOffset.UtcNow;
            simpleRangeEvent.Id ??= MlrbId.From(simpleRangeEvent.EventDate);

            try
            {
                var p = new
                {
                    Id = simpleRangeEvent.Id!,
                    EventDate = simpleRangeEvent.EventDate,
                    FirearmName = simpleRangeEvent.FirearmName,
                    RangeName = simpleRangeEvent.RangeName,
                    RoundsFired = simpleRangeEvent.RoundsFired,
                    AmmoDescription = simpleRangeEvent.AmmoDescription,
                    Notes = simpleRangeEvent.Notes,
                    Created = simpleRangeEvent.Created,
                    Modified = simpleRangeEvent.Modified
                };
                var ctx = new DapperCommandContext(connection, transaction, cancellationToken, p);
                long result = await UpsertCommand.ExecuteScalarAsync<long>(ctx).ConfigureAwait(false);

                simpleRangeEvent.RowId = result;

                var reason = new Success($"SimpleRangeEvent `{simpleRangeEvent.Id}` saved.");
                reason.WithMetadata("Id", simpleRangeEvent.Id);
                reason.WithMetadata("RowId", simpleRangeEvent.RowId);
                reason.WithMetadata("Database", connection.Database);


                return Result.Ok(simpleRangeEvent.RowId).WithSuccess(reason);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not save SimpleRangeEvent `{simpleRangeEvent.Id}`: {e.Message}");
                err.CausedBy(e).Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId);

                return Result.Fail(err);
            }
        }
    }
}
