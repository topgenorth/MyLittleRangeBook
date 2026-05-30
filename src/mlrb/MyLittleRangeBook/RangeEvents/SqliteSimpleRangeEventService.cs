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
                                 INSERT INTO SimpleRangeEvents (Id, EventDate, FirearmName, RangeName, RoundsFired, AmmoDescription, Notes, Created, Modified)
                                 VALUES (@Id, @EventDate, @FirearmName, @RangeName, @RoundsFired, @AmmoDescription, @Notes, @Created, @Modified)
                                 ON CONFLICT(Id) DO UPDATE SET
                                   EventDate = excluded.EventDate,
                                   FirearmName = excluded.FirearmName,
                                   RangeName = excluded.RangeName,
                                   RoundsFired = excluded.RoundsFired,
                                   AmmoDescription = excluded.AmmoDescription,
                                   Notes = excluded.Notes,
                                   Modified = excluded.Modified
                                 RETURNING RowId;
                                 """;

        const string SelectSql = """
                                 SELECT *
                                 FROM SimpleRangeEvents 
                                 ORDER BY EventDate, FirearmName, RangeName;
                                 """;

        const string DeleteSql = "DELETE FROM SimpleRangeEvents WHERE Id = @Id;";

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
