using System.Data.Common;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.Firearms.IFirearmAggregateRepository;

namespace MyLittleRangeBook.Firearms
{
    public class SqliteFirearmAggregateRepository : SqliteAggregateRepository<FirearmAggregate>,
                                                    IFirearmAggregateRepository
    {
        public SqliteFirearmAggregateRepository(ISqliteHelper sqliteHelper, IEventSerializer eventSerializer) :
            base(sqliteHelper,
                 eventSerializer,
                 FirearmAggregate.STREAM_TYPE,
                 FirearmAggregate.Create) { }

        public async Task<Result<FirearmAggregate>> GetAsync(DapperCommandContext context, MlrbId firearmId)
        {
            return await base.GetAsync(context, firearmId).ConfigureAwait(false);
        }

        public async Task<Result<FirearmAggregate>> GetOrCreateByNameAsync(string firearmName,
                                                                           CancellationToken cancellationToken =
                                                                               default,
                                                                           DateTimeOffset? createUtc = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(firearmName);

            await using var ctx = await DapperCommandContext.NewAsync(SqliteHelper, cancellationToken);

            Result<FirearmAggregate> r1 = await GetOrCreateByNameAsync(ctx, firearmName, createUtc);
            if (r1.IsFailed)
            {
                ctx.Transaction!.Rollback();
                return Result.Fail(r1.Errors);
            }

            return Result.Ok(r1.Value);
        }

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

        public async Task<IEnumerable<RangeEventRoundCountRow>> GetSimpleRangeEventRoundCountsByFirearmNameAsync(
            DapperCommandContext context, string name)
        {
            DapperCommandContext ctx = context with { Arguments = new { FirearmName = name } };
            IEnumerable<RangeEventRoundCountRow> list = await Commands
                                                             .GetSimpleRangeEventsNotAssociatedWithFirearms
                                                             .QueryAsync<RangeEventRoundCountRow>(ctx)
                                                             .ConfigureAwait(false);

            return list;
        }

        public async Task<Result> SaveAsync(FirearmAggregate aggregate, CancellationToken cancellationToken = default)
        {
            await using var ctx = await DapperCommandContext.NewAsync(SqliteHelper, cancellationToken);
            return await UpsertAsync(ctx, aggregate);
        }


        public async Task<Result<IEnumerable<NewFirearmNameFromSimpleRangeEventRow>>>
            GetNewFirearmNamesFromSimpleRangeEventsAsync(DapperCommandContext context)
        {
            try
            {
                IEnumerable<NewFirearmNameFromSimpleRangeEventRow> firearms = await Commands
                                                                                 .NewFirearmNamesFromRangeEvents
                                                                                 .QueryAsync<
                                                                                      NewFirearmNameFromSimpleRangeEventRow>(context)
                                                                                 .ConfigureAwait(false);
                return Result.Ok(firearms);
            }
            catch (Exception e)
            {
                Error? err = new Error("Unexpected exception trying to get a list of new firearm names").CausedBy(e);
                return Result.Fail(err);
            }
        }



        public async Task<IList<RangeEventRoundCountRow>>
            GetSimpleRangeEventsForFirearmName(DapperCommandContext context, string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            DapperCommandContext ctx = context with { Arguments = new { FirearmName = name } };
            IEnumerable<RangeEventRoundCountRow> list = await Commands.GetSimpleRangeEventRoundCountsByFirearmName
                                                                      .QueryAsync<RangeEventRoundCountRow>(ctx)
                                                                      .ConfigureAwait(false);
            return list.ToList();
        }

        static class Commands
        {
            /// <summary>
            ///     Retrieves the names of firearms and their associated total rounds fired,
            ///     based on range events, for firearms that are not yet present in the firearms table.
            ///     The results are grouped and ordered by firearm name.
            /// </summary>
            const string GetNewFirearmNamesFromRangeEventsSql = """
                                                                WITH UnassociatedOldest AS (
                                                                    SELECT s.id           AS SimpleRangeEventId,
                                                                           s.firearm_name AS FirearmName,
                                                                           s.event_date   AS Created,
                                                                           ROW_NUMBER() OVER (PARTITION BY s.firearm_name ORDER BY s.event_date, s.id) AS rn
                                                                    FROM simple_range_events s
                                                                    WHERE NOT EXISTS (SELECT 1
                                                                                      FROM events e
                                                                                      WHERE e.stream_type = 'firearm'
                                                                                        AND e.event_type = 'firearm-created'
                                                                                        AND JSON_EXTRACT(e.data_json, '$.name') = s.firearm_name)
                                                                       OR NOT EXISTS (SELECT 1
                                                                                      FROM firearms f
                                                                                      WHERE f.name = s.firearm_name)
                                                                )
                                                                SELECT SimpleRangeEventId,
                                                                       FirearmName,
                                                                       Created
                                                                FROM UnassociatedOldest
                                                                WHERE rn = 1
                                                                ORDER BY Created, FirearmName;
                                                                """;

            const string GetSimpleRangeEventsNotAssociatedWithFirearmsSql = """
                                                                            SELECT s.id AS SimpleRangeEventId,
                                                                                   s.firearm_name AS FirearmName,
                                                                                   s.rounds_fired AS RoundsFired,
                                                                                   s.event_date AS EventDate
                                                                            FROM simple_range_events s
                                                                            WHERE NOT EXISTS (SELECT 1
                                                                                              FROM events e
                                                                                              WHERE e.stream_type = 'firearm'
                                                                                                AND e.event_type = 'range-event-associated-with-firearm'
                                                                                                AND JSON_EXTRACT(e.data_json, '$.rangeEventId') = s.id)
                                                                            AND s.firearm_name = @FirearmName
                                                                            ORDER BY s.event_date, s.firearm_name;

                                                                            """;

            const string GetSimpleRangeEventRoundCountsByFirearmNameSql = """
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
            ///     A pre-defined database command that retrieves information about firearms from range events
            ///     that have not yet been associated with a created firearm in the system. Each result
            ///     includes the firearm name, the earliest event date, and the identifier for that event,
            ///     ensuring that only the first unassociated event per firearm is selected. Results are
            ///     ordered by the event date and firearm name.
            /// </summary>
            internal static readonly DapperCommand NewFirearmNamesFromRangeEvents =
                new(GetNewFirearmNamesFromRangeEventsSql);


            /// <summary>
            ///     Get a list of SimpleRangeEvents and the rounds fired for a given firearm name.
            /// </summary>
            internal static readonly DapperCommand GetSimpleRangeEventRoundCountsByFirearmName =
                new(GetSimpleRangeEventRoundCountsByFirearmNameSql);

            /// <summary>
            ///     A list of all range events (that are not associated with a firearm aggregrete.
            /// </summary>
            internal static readonly DapperCommand GetSimpleRangeEventsNotAssociatedWithFirearms =
                new(GetSimpleRangeEventsNotAssociatedWithFirearmsSql);
        }
    }
}