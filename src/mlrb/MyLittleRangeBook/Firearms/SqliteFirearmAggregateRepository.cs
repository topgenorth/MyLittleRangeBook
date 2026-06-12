using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Firearms
{
    public interface IFirearmAggregateRepository
    {
        /// <summary>
        /// Will retrieve a firearm aggregate by its name, creating it if it doesn't exist.
        /// </summary>
        /// <param name="firearmName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<FirearmAggregate>> GetOrCreateByNameAsync(string firearmName,
            CancellationToken cancellationToken = default);

        Task<Result> SaveAsync(FirearmAggregate aggregate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a firearm aggregate by the id.
        /// </summary>
        /// <param name="firearmId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<FirearmAggregate?>> GetAsync(MlrbId firearmId, CancellationToken cancellationToken = default);
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

        public async Task<Result<FirearmAggregate>> GetOrCreateByNameAsync(string firearmName,
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
