using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.RangeEvents
{
    public class SimpleRangeEventRepositoryFirearmStream : ISimpleRangeEventRepository
    {
        readonly SqliteSimpleRangeEventRepository _wrapped;
        readonly IFirearmAggregateRepository      _faRepo;
        readonly ISqliteHelper                    _sqliteHelper;
        public SimpleRangeEventRepositoryFirearmStream(SqliteSimpleRangeEventRepository wrapped, IFirearmAggregateRepository faRepo, ISqliteHelper sqliteHelper)
        {
            _wrapped           = wrapped;
            _faRepo            = faRepo;
            _sqliteHelper = sqliteHelper;
        }

        public Task<Result> DeleteAsync(SimpleRangeEvent  simpleRangeEvent,
                                        CancellationToken cancellationToken = default) => _wrapped.DeleteAsync(simpleRangeEvent, cancellationToken);

        public async Task<Result<long>> UpsertAsync(SimpleRangeEvent  simpleRangeEvent,
                                                     CancellationToken cancellationToken = default)
        {

            await using var scopedConn = await _sqliteHelper
                                              .GetScopedDatabaseConnectionAsync(cancellationToken)
                                                .ConfigureAwait(false);
            await using var trans = await scopedConn.Connection
                                                    .BeginTransactionAsync(cancellationToken)
                                                    .ConfigureAwait(false);
            var ctx = new DapperCommandContext(scopedConn.Connection, trans, cancellationToken);

            var r1 = await _wrapped.UpsertAsync(ctx, simpleRangeEvent);
            if (r1.IsFailed)
                return r1;

            var r2 = await AddToFirearmStream(ctx, simpleRangeEvent);
            if (r2.IsFailed)
            {
                await trans.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return Result.Fail(r2.Errors[0]);
            }
            await trans.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Result.Ok();
        }

        async Task<Result> AddToFirearmStream(DapperCommandContext ctx, SimpleRangeEvent sre)
        {
            var r1 = await _faRepo.GetOrCreateByNameAsync(ctx, sre.FirearmName);
            if (r1.IsFailed)
            {
                return Result.Fail(r1.Errors);
            }

            var fa = r1.Value;
            fa.AssociateWithSimpleRangeEvent(sre.Id!, sre.Created);
            if (sre.RoundsFired> 0)
            {
                fa.MoreRoundsFired(sre.RoundsFired, sre.EventDate);
            }

            var r2 = await _faRepo.SaveAsync(ctx, fa).ConfigureAwait(false);
            return r2.IsFailed ? Result.Fail(r2.Errors) : Result.Ok();
        }

        public async Task<Result<long>> UpsertAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent)
        {
            var r1 = await _wrapped.UpsertAsync(context, simpleRangeEvent);
            if (r1.IsFailed)
            {
                return Result.Fail(r1.Errors[0]);
            }

            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default) => _wrapped.GetSimpleRangeEventsAsync(cancellationToken);

        public Task<Result<SimpleRangeEvent>> GetAsync(string id, CancellationToken cancellationToken = default) =>
            _wrapped.GetAsync(id, cancellationToken);
    }
}