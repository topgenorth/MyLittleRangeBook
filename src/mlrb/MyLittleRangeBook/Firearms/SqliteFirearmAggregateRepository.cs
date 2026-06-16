using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Firearms
{
    public class SqliteFirearmAggregateRepository : SqliteAggregateRepository<FirearmAggregate>,
        IFirearmAggregateRepository
    {
        public SqliteFirearmAggregateRepository(ISqliteHelper sqliteHelper, IEventSerializer eventSerializer) :
            base(sqliteHelper,
                eventSerializer,
                FirearmAggregate.STREAM_TYPE,
                FirearmAggregate.Create)
        {
        }

        public async Task<IList<IFirearmAggregateRepository.RangeEventRoundCountRow>>
            GetSimpleRangeEventsForFirearmName(DapperCommandContext context, string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            var ctx = context with { Arguments = new { FirearmName = name } };
            var list = await Commands.GetSimpleRangeEventRoundCountsByFirearmName
                .QueryAsync<IFirearmAggregateRepository.RangeEventRoundCountRow>(ctx)
                .ConfigureAwait(false);
            return list.ToList();
        }

        public async Task<Result<FirearmAggregate>> GetOrCreateByNameAsync(string firearmName,
            CancellationToken cancellationToken = default,
            DateTimeOffset? createUtc = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(firearmName);
            var streamId = MlrbId.FromString(firearmName);
            var firearmAggregate = await GetAsync(streamId, cancellationToken);

            if (firearmAggregate.IsFailed)
            {
                return Result.Fail(firearmAggregate.Errors);
            }

            if (firearmAggregate.Value is not null)
            {
                return Result.Ok(firearmAggregate.Value);
            }

            var createdUtc = createUtc ?? DateTimeOffset.UtcNow;
            var fa = FirearmAggregate.New(firearmName, createdUtc);

            return Result.Ok(fa);
        }

        public Task<IList<IFirearmAggregateRepository.RangeEventRoundCountRow>> GetSimpleRangeEventRoundCountsByFirearmNameAsync(DapperCommandContext context, string name)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IFirearmAggregateRepository.NewFirearmNameFromSimpleRangeEventRow>>
            GetNewFirearmNamesFromSimpleRangeEventsAsync(DapperCommandContext context)
        {
            var firearms = await Commands
                .NewFirearmNamesFromRangeEvents
                .QueryAsync<IFirearmAggregateRepository.NewFirearmNameFromSimpleRangeEventRow>(context)
                .ConfigureAwait(false);
            return firearms;
        }

        private static class Commands
        {
            /// <summary>
            ///     Retrieves the names of firearms and their associated total rounds fired,
            ///     based on range events, for firearms that are not yet present in the firearms table.
            ///     The results are grouped and ordered by firearm name.
            /// </summary>
            private const string GetNewFirearmNamesFromRangeEventsSql = """
                                                                        WITH UnassociatedOldest AS (
                                                                            SELECT s.id           AS SimpleRangeEventId,
                                                                                   s.firearm_name AS FirearmName,
                                                                                   s.event_date   AS EventDate,
                                                                                   ROW_NUMBER() OVER (PARTITION BY s.firearm_name ORDER BY s.event_date, s.id) AS rn
                                                                            FROM simple_range_events s
                                                                            WHERE NOT EXISTS (SELECT 1
                                                                                              FROM events e
                                                                                              WHERE e.stream_type = 'firearm'
                                                                                                AND e.event_type = 'firearm-created'
                                                                                                AND JSON_EXTRACT(e.data_json, '$.name') = s.firearm_name)
                                                                        )
                                                                        SELECT SimpleRangeEventId,
                                                                               FirearmName,
                                                                               EventDate
                                                                        FROM UnassociatedOldest
                                                                        WHERE rn = 1
                                                                        ORDER BY EventDate, FirearmName;
                                                                        """;

            private const string GetSimpleRangeEventRoundCountsByFirearmNameSql = """

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


            internal static readonly DapperCommand GetSimpleRangeEventRoundCountsByFirearmName =
                new(GetSimpleRangeEventRoundCountsByFirearmNameSql);
        }
    }
}