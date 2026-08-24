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
    public class SqliteFirearmAggregateRepository : SqliteAggregateRepository<Firearm>,
                                                    IFirearmAggregateRepository
    {
        public SqliteFirearmAggregateRepository(ISqliteHelper         sqliteHelper,
                                                IEventSerializer      eventSerializer,
                                                IEventSourcingService eventSourcingService) :
            base(sqliteHelper,
                 eventSerializer,
                 Firearm.STREAM_TYPE,
                 Firearm.Create, eventSourcingService, null
                 ) { }

        public async Task<Result<Firearm>> GetOrCreateByNameAsync(DapperCommandContext ctx,
                                                                           string               firearmName,
                                                                           DateTimeOffset?      createUtc = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(firearmName);

            MlrbId                    streamId          = MlrbId.FromString(firearmName);
            DateTimeOffset            createdUtc        = createUtc ?? DateTimeOffset.UtcNow;
            Result<Firearm>  result;

            Result<Firearm?> rFirearmAggregate = await GetAsync(ctx, streamId);
            if (rFirearmAggregate.HasError<EventStreamDoesNotExistError>())
            {
                Firearm fa = Firearm.New(firearmName, createdUtc);
                result = Result.Ok(fa).WithReason(new FirearmEventStreamCreatedReason(firearmName, streamId));
            }
            else if (rFirearmAggregate.IsFailed)
            {
                FailedToGetFirearmEventStream err = new(firearmName, streamId);
                result = Result.Fail(err.Message).WithReasons(rFirearmAggregate.Reasons);
            }
            else if (rFirearmAggregate.Value is not null)
            {
                Firearm? fa = rFirearmAggregate.Value;
                result = Result.Ok(fa)
                               .WithReason(new FirearmEventStreamCreatedReason(firearmName, streamId));
            }
            else
            {
                result = Result.Fail<Firearm>(new FailedToGetFirearmEventStream(firearmName, streamId));
            }

            return result;
        }
    }
}