using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    ///     Provides a repository for managing and accessing firearm aggregates using SQLite as the underlying data store.
    ///     This repository implements functionality for retrieving, creating, and saving firearm aggregates,
    ///     as well as querying firearm-related data.
    /// </summary>
    /// <remarks>
    ///     Inherits from the <see cref="SqliteAggregateRepository{TAggregate}" /> to provide base functionality for
    ///     SQLite-based event-sourced aggregate repositories. Implements the <see cref="IFirearmAggregateRepository" />
    ///     interface for firearm-specific repository operations.
    /// </remarks>
    public class SqliteFirearmAggregateRepository : SqliteAggregateRepository<FirearmAggregate>,
                                                    IFirearmAggregateRepository
    {
        public SqliteFirearmAggregateRepository(ISqliteHelper         sqliteHelper,
                                                IEventSerializer      eventSerializer,
                                                IEventSourcingService eventSourcingService) :
            base(sqliteHelper,
                 eventSerializer,
                 FirearmAggregate.STREAM_TYPE,
                 FirearmAggregate.Create, eventSourcingService) { }

        public async Task<Result<FirearmAggregate>> GetOrCreateByNameAsync(DapperCommandContext ctx,
                                                                           string               firearmName,
                                                                           DateTimeOffset?      createUtc = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(firearmName);

            MlrbId                    streamId         = MlrbId.FromString(firearmName);
            Result<FirearmAggregate?> firearmAggregate = await GetAsync(ctx, streamId);

            if (firearmAggregate.IsFailed)
            {
                return Result.Fail(firearmAggregate.Errors);
            }

            if (firearmAggregate.Value is not null)
            {
                return Result.Ok(firearmAggregate.Value);
            }

            DateTimeOffset   createdUtc = createUtc ?? DateTimeOffset.UtcNow;
            FirearmAggregate fa         = FirearmAggregate.New(firearmName, createdUtc);

            return Result.Ok(fa);
        }


        public async Task<Result> SaveAsync(FirearmAggregate aggregate, CancellationToken cancellationToken = default)
        {
            await using DapperCommandContext ctx =
                await DapperCommandContext.NewAsync(SqliteHelper, cancellationToken, true);
            Result r1 = await UpsertAsync(ctx, aggregate);
            if (r1.IsSuccess)
            {
                await ctx.CommitAsync().ConfigureAwait(false);
            }
            else
            {
                await ctx.RollbackAsync().ConfigureAwait(false);
            }

            return r1;
        }
    }
}