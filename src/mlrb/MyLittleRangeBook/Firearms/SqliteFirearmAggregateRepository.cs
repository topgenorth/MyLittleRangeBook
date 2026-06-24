using System.Data.Common;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.Firearms.IFirearmAggregateRepository;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    /// Provides a repository for managing and accessing firearm aggregates using SQLite as the underlying data store.
    /// This repository implements functionality for retrieving, creating, and saving firearm aggregates,
    /// as well as querying firearm-related data.
    /// </summary>
    /// <remarks>
    /// Inherits from the <see cref="SqliteAggregateRepository{TAggregate}"/> to provide base functionality for
    /// SQLite-based event-sourced aggregate repositories. Implements the <see cref="IFirearmAggregateRepository"/>
    /// interface for firearm-specific repository operations.
    /// </remarks>
    public class SqliteFirearmAggregateRepository : SqliteAggregateRepository<FirearmAggregate>,
                                                    IFirearmAggregateRepository
    {
        public SqliteFirearmAggregateRepository(ISqliteHelper sqliteHelper, IEventSerializer eventSerializer) :
            base(sqliteHelper,
                 eventSerializer,
                 FirearmAggregate.STREAM_TYPE,
                 FirearmAggregate.Create) { }

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
            await using var ctx = await DapperCommandContext.NewAsync(SqliteHelper, cancellationToken);
            return await UpsertAsync(ctx, aggregate);
        }


        static class Commands
        {



            const string GET_SIMPLE_RANGE_EVENT_ROUND_COUNTS_BY_FIREARM_NAME_SQL = """
                                                                          SELECT s.id           AS SimpleRangeEventId,
                                                                                 s.firearm_name AS FirearmName,
                                                                                 s.rounds_fired AS RoundsFired,
                                                                                 s.event_date   AS EventDate
                                                                          FROM simple_range_events s
                                                                          WHERE s.rounds_fired > 0
                                                                            AND s.firearm_name = @FirearmName
                                                                          ORDER BY s.event_date, s.firearm_name;
                                                                          """;





            /// <summary>
            ///     Get a list of SimpleRangeEvents and the rounds fired for a given firearm name.
            /// </summary>
            internal static readonly DapperCommand s_getSimpleRangeEventRoundCountsByFirearmName =
                new(GET_SIMPLE_RANGE_EVENT_ROUND_COUNTS_BY_FIREARM_NAME_SQL);


        }
    }
}