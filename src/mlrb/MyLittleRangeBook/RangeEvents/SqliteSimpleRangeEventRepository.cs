using System.Data.Common;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.Persistence.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Represents a repository implementation for managing simple range events using SQLite as the persistence layer.
    /// </summary>
    public class SqliteSimpleRangeEventRepository : ISimpleRangeEventRepository
    {
        readonly IFirearmAggregateRepository _faRepo;
        readonly ISimpleRangeEventService    _simpleRangeEventService;
        readonly ISqliteHelper               _sqliteHelper;

        public SqliteSimpleRangeEventRepository(ISqliteHelper sqliteHelper,
                                                [FromKeyedServices(DI_KEYS_SQLITE)]
                                                ISimpleRangeEventService simpleRangeEventService,
                                                IFirearmAggregateRepository faRepo)
        {
            _sqliteHelper            = sqliteHelper;
            _simpleRangeEventService = simpleRangeEventService;
            _faRepo                  = faRepo;
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
            await using DbTransaction    t = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                Result<IEnumerable<SimpleRangeEvent>> result =
                    await _simpleRangeEventService.GetSimpleRangeEventsAsync(conn, t, cancellationToken);
                if (!result.IsFailed)
                {
                    return result;
                }

                await t.RollbackAsync(cancellationToken).ConfigureAwait(false);

                return Result.Fail(result.Errors);
            }
            catch (Exception e)
            {
                await t.RollbackAsync(cancellationToken).ConfigureAwait(false);
                Error err = new Error("Failed to retrieve simple range events.").CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result<SimpleRangeEvent>> GetAsync(string id, CancellationToken cancellationToken)
        {
            await using SqliteConnection conn = await _sqliteHelper
                                                     .GetDatabaseConnectionAsync(cancellationToken)
                                                     .ConfigureAwait(false);

            try
            {
                SimpleRangeEvent? sre = await conn
                                           .QueryFirstOrDefaultAsync<SimpleRangeEvent>(Commands.GetByIdSql,
                                                new { Id = id });
                if (sre is not null)
                {
                    return Result.Ok(sre);
                }

                Error err = new Error("Could not find range event " + id + ".").Enrich(id);
                return Result.Fail(err);
            }
            catch (Exception ex)
            {
                Error err = new Error(ex.Message).CausedBy(ex).Enrich(id);

                return Result.Fail(err);
            }
        }

        public async Task<Result> DeleteAsync(SimpleRangeEvent  simpleRangeEvent,
                                              CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
            await using DbTransaction trans = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                return await _simpleRangeEventService.DeleteAsync(conn, simpleRangeEvent, trans, cancellationToken)
                                                     .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await trans.RollbackAsync(cancellationToken).ConfigureAwait(false);
                Error? err = new Error("Failed to delete simple range event.")
                            .Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId)
                            .CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result<long>> UpsertAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent)
        {
            Result<long?> sreResult = await _simpleRangeEventService
                                           .UpsertAsync(context.Connection, simpleRangeEvent, context.Transaction,
                                                        context.CancellationToken)
                                           .ConfigureAwait(false);
            if (sreResult.IsFailed || sreResult.Value is null)
            {
                return Result.Fail(sreResult.Errors);
            }

            Result r1 = await AddToFirearmStream(context, simpleRangeEvent);

            return Result.Fail(r1.IsFailed ? r1.Errors : sreResult.Errors);
        }


        public async Task<Result<long>> UpsertAsync(SimpleRangeEvent  simpleRangeEvent,
                                                    CancellationToken cancellationToken = default)
        {
            DapperCommandContext ctx = await DapperCommandContext
                                            .NewAsync(_sqliteHelper, cancellationToken)
                                            .ConfigureAwait(false);

            Result<long> r1 = await UpsertAsync(ctx, simpleRangeEvent).ConfigureAwait(false);

            if (r1.IsFailed)
            {
                ctx.Transaction?.Rollback();
            }

            return r1;
        }

        async Task<Result> AddToFirearmStream(DapperCommandContext ctx, SimpleRangeEvent sre)
        {
            Result<FirearmAggregate> r1 = await _faRepo.GetOrCreateByNameAsync(ctx, sre.FirearmName);
            if (r1.IsFailed)
            {
                return Result.Fail(r1.Errors);
            }

            FirearmAggregate? fa = r1.Value;
            fa.AssociateWithSimpleRangeEvent(sre.Id!, sre.Created);
            if (sre.RoundsFired > 0)
            {
                fa.MoreRoundsFired(sre.RoundsFired, sre.EventDate);
            }

            Result r2 = await _faRepo.UpsertAsync(ctx, fa).ConfigureAwait(false);
            return r2.IsFailed ? Result.Fail(r2.Errors) : Result.Ok();
        }

        static class Commands
        {
            internal const string GetByIdSql = """
                                               SELECT row_id AS RowId,
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
                                               WHERE id=@Id;
                                               """;
        }
    }
}