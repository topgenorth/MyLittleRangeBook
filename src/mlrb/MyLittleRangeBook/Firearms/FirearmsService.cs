using Fisher;
using Fisher.Exceptions;
using JasperFx.Events;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmsService : IFirearmsService
    {
        readonly ILogger          _logger;
        readonly IDocumentSession _session;

        public FirearmsService(ILogger logger, IDocumentSession session)
        {
            _logger  = logger;
            _session = session;
        }

        public Task<Result> DeleteAsync(FirearmTableRow firearmTableRow) => throw new NotImplementedException();

        public Task<Result> DeleteAsync(MlrbId firearmId) => throw new NotImplementedException();

        public Task<Result<FirearmTableRow>> GetFirearmAsync(MlrbId id) => throw new NotImplementedException();

        public Task<Result<IEnumerable<FirearmTableRow>>> GetFirearmsAsync(bool activeOnly = true) =>
            throw new NotImplementedException();

        public Task<Result<MlrbId>> UpsertAsync(FirearmTableRow firearmTableRow) => throw new NotImplementedException();

        /// <summary>
        ///     Append a new <code>GarminShotViewFileAddedToFirearm</code> event to the firearm stream.
        /// </summary>
        /// <param name="firearmName"></param>
        /// <param name="fileContents"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result> AddGarminShotviewCsv(string            firearmName, string fileContents,
                                                       CancellationToken cancellationToken = default)
        {
            GarminShotViewFileAddedToFirearm e = new(firearmName, fileContents, DateTimeOffset.UtcNow);

            Result<Guid> r = await FetchStreamIdForFirearm(firearmName, cancellationToken).ConfigureAwait(false);
            if (r.IsFailed)
            {
                return Result.Fail(r.Errors);
            }

            _session.Events.Append(r.Value, e);
            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Ok();
        }

        /// <summary>
        /// Try to get the stream ID for the firearm name.  If it doesn't exist, then create the stream.
        /// </summary>
        /// <param name="firearmName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<Result<Guid>> FetchStreamIdForFirearm(string            firearmName,
                                                           CancellationToken cancellationToken)
        {
            Guid firearmId;
            bool create = false;
            try
            {
                IEventStream<Firearm> stream =
                    await _session.Events.FetchForWritingByNaturalKey<Firearm, string>(firearmName,
                             cancellationToken);
                firearmId = stream.Id;
            }
            catch (UnknownNaturalKeyException)
            {
                _logger.Verbose("{0} is not a known natural key.", firearmName);
                create    = true;
                firearmId = Guid.CreateVersion7();
            }

            if (!create)
            {
                return firearmId;
            }

            try
            {
                _session.Events.StartStream<Firearm>(firearmId,
                                                     new FirearmCreated(firearmName, DateTimeOffset.UtcNow));
                _logger.Verbose("Created the stream for natural key {0}/{1}.", firearmName, firearmId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while creating the firearm stream for natural key {0}.",
                              firearmName);
                return Result.Fail(ex.ToError());
            }

            return firearmId;
        }
    }
}