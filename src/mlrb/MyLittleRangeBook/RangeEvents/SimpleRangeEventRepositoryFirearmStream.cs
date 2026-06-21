using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.RangeEvents
{
    public class SimpleRangeEventRepositoryFirearmStream : ISimpleRangeEventRepository
    {
        readonly IFirearmAggregateRepository      _faRepo;
        readonly ISqliteHelper                    _sqliteHelper;
        readonly SqliteSimpleRangeEventRepository _wrapped;

        public SimpleRangeEventRepositoryFirearmStream(SqliteSimpleRangeEventRepository wrapped,
                                                       IFirearmAggregateRepository faRepo, ISqliteHelper sqliteHelper)
        {
            _wrapped      = wrapped;
            _faRepo       = faRepo;
            _sqliteHelper = sqliteHelper;
        }

        public Task<Result> DeleteAsync(SimpleRangeEvent  simpleRangeEvent,
                                        CancellationToken cancellationToken = default) =>
            _wrapped.DeleteAsync(simpleRangeEvent, cancellationToken);

        /// <summary>
        ///     Inserts or updates a <see cref="SimpleRangeEvent" /> in the database and logs the event into the firearm stream.
        /// </summary>
        /// <param name="simpleRangeEvent">
        ///     The <see cref="SimpleRangeEvent" /> to be inserted or updated in the database.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        ///     A <see cref="Result{T}" /> containing the identifier of the updated or inserted record upon success,
        ///     or an error result if the operation failed.
        /// </returns>
        public async Task<Result<long>> UpsertAsync(SimpleRangeEvent  simpleRangeEvent,
                                                    CancellationToken cancellationToken = default)
        {
            await using ScopedSqliteConnection scopedConn = await _sqliteHelper
                                                                 .GetScopedDatabaseConnectionAsync(cancellationToken,
                                                                           true)
                                                                 .ConfigureAwait(false);
            DapperCommandContext ctx = new(scopedConn, cancellationToken);

            Result<long> r1 = await UpsertAsync(ctx, simpleRangeEvent);
            if (r1.IsFailed)
            {
                await scopedConn.Transaction!.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return r1;
            }

            await scopedConn.Transaction!.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Result.Ok();
        }

        /// <summary>
        ///     Inserts or updates a <see cref="SimpleRangeEvent" /> in the database and logs the event into the firearm stream.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="DapperCommandContext" /> representing the database connection, transaction, and other related
        ///     arguments.
        /// </param>
        /// <param name="simpleRangeEvent">
        ///     The <see cref="SimpleRangeEvent" /> to be inserted or updated in the database.
        /// </param>
        /// <returns>
        ///     A <see cref="Result{T}" /> containing the identifier of the updated or inserted record upon success,
        ///     or an error result if the operation failed.
        /// </returns>
        public async Task<Result<long>> UpsertAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent)
        {
            Result<long> r1 = await _wrapped.UpsertAsync(context, simpleRangeEvent);
            if (r1.IsFailed)
            {
                return r1;
            }

            Result r2 = await AddToFirearmStream(context, simpleRangeEvent);
            if (r2.IsFailed)
            {
                return Result.Fail(r2.Errors);
            }

            return r1;
        }

        public Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default) => _wrapped.GetSimpleRangeEventsAsync(cancellationToken);

        public Task<Result<SimpleRangeEvent>> GetAsync(string id, CancellationToken cancellationToken = default) =>
            _wrapped.GetAsync(id, cancellationToken);

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
    }
}