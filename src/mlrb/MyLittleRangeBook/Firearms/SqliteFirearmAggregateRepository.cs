using Microsoft.Extensions.DependencyInjection;
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
                                                IEventSourcingService eventSourcingService,
                                                [FromKeyedServices(FirearmProjector.DI_KEY)]
                                                IProjector projector) :
            base(sqliteHelper,
                 eventSerializer,
                 FirearmAggregate.STREAM_TYPE,
                 FirearmAggregate.Create, eventSourcingService,
                 projector) { }

        public async Task<Result<FirearmAggregate>> GetOrCreateByNameAsync(DapperCommandContext ctx,
                                                                           string               firearmName,
                                                                           DateTimeOffset?      createUtc = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(firearmName);

            MlrbId                    streamId          = MlrbId.FromString(firearmName);
            DateTimeOffset            createdUtc        = createUtc ?? DateTimeOffset.UtcNow;
            Result<FirearmAggregate>  result;

            Result<FirearmAggregate?> rFirearmAggregate = await GetAsync(ctx, streamId);
            if (rFirearmAggregate.HasError<EventStreamDoesNotExistError>())
            {
                FirearmAggregate fa = FirearmAggregate.New(firearmName, createdUtc);
                result = Result.Ok(fa).WithReason(new FirearmEventStreamCreatedReason(firearmName, streamId));
            }
            else if (rFirearmAggregate.IsFailed)
            {
                FailedToGetFirearmEventStream err = new(firearmName, streamId);
                result = Result.Fail(err.Message).WithReasons(rFirearmAggregate.Reasons);
            }
            else if (rFirearmAggregate.Value is not null)
            {
                FirearmAggregate? fa = rFirearmAggregate.Value;
                result = Result.Ok(fa)
                               .WithReason(new FirearmEventStreamCreatedReason(firearmName, streamId));
            }
            else
            {
                result = Result.Fail<FirearmAggregate>(new FailedToGetFirearmEventStream(firearmName, streamId));
            }

            return result;
        }
    }
}