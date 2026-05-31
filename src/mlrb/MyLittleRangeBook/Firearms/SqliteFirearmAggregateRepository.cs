using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Firearms
{
    public interface IFirearmAggregateRepository
    {
        Task<Result<FirearmAggregate>> GetByNameAsync(string firearmName,
            CancellationToken cancellationToken = default);
    }

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

        public async Task<Result<FirearmAggregate>> GetByNameAsync(string firearmName,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(firearmName);
            var streamId = MlrbId.FromString(firearmName);
            Result<FirearmAggregate?> firearmAggregate = await GetAsync(streamId, cancellationToken);

            if (firearmAggregate.IsFailed)
            {
                return Result.Fail(firearmAggregate.Errors);
            }

            if (firearmAggregate.Value is not null)
            {
                return Result.Ok(firearmAggregate.Value);
            }

            var fa = FirearmAggregate.New(firearmName, 0, null, DateTimeOffset.UtcNow);

            return Result.Ok(fa);
        }
    }
}
