using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    ///     Will update the Firearms table with the events from a given stream.
    /// </summary>
    internal class FirearmsProjector : IProjector
    {
        private readonly ILogger _logger;
        private readonly IFirearmAggregateRepository _repo;
        private readonly IFirearmsService _service;
        private readonly ISqliteHelper _sqliteHelper;

        public FirearmsProjector(ISqliteHelper sqliteHelper, IFirearmsService service, IFirearmAggregateRepository repo,
            ILogger logger)
        {
            _sqliteHelper = sqliteHelper;
            _service = service;
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result> ProjectAggregateAsync(DapperCommandContext context,
            MlrbId streamId,
            IEnumerable<IDomainEvent>? domainEvents = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var fa = await _repo.GetAsync(streamId, cancellationToken).ConfigureAwait(false);
                if (fa.IsFailed)
                {
                    _logger.Warning("Failed to retrieve firearm aggregate. {0}", fa.Errors[0]);
                    return Result.Fail(fa.Errors);
                }

                var scopedConn = await _sqliteHelper
                    .GetScopedDatabaseConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
                var ctx = new DapperCommandContext(scopedConn.Connection, CancellationToken: cancellationToken);
                var x = await _service.UpsertAsync(ctx, fa.Value!).ConfigureAwait(false);

                if (x.IsFailed)
                {
                    _logger.Error("Failed to updated the firearm projection. {0}", x.Errors[0]);
                    return Result.Fail(x.Errors);
                }

                return Result.Ok();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unexpected error with the firearm aggregate projection.");
                var err = new Error("Unexpected error with the firearm aggregate projection.").CausedBy(e);
                return Result.Fail(err);
            }
        }
    }
}