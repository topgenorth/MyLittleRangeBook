using Fisher;
using Fisher.Exceptions;
using JasperFx.Events;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public class FisherFirearmsService : IFirearmsService
    {
        readonly ILogger          _logger;
        readonly IDocumentSession _session;

        public FisherFirearmsService(ILogger logger, IDocumentSession session)
        {
            _logger  = logger;
            _session = session;
        }

        [Obsolete("Not in use.")]
        public Task<Result> DeleteAsync(FirearmTableRow firearmTableRow) => throw new NotImplementedException();

        [Obsolete("Not in use.")]
        public Task<Result> DeleteAsync(MlrbId firearmId) => throw new NotImplementedException();

        [Obsolete("Not in use.")]
        public Task<Result<FirearmTableRow>> GetFirearmAsync(MlrbId id) => throw new NotImplementedException();

        public Task<Result<IEnumerable<FirearmTableRow>>> GetFirearmsAsync(bool activeOnly = true) =>
            throw new NotImplementedException();

        [Obsolete("Not in use.")]
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
        /// Adds a new reloading recipe to the specified firearm's event stream.
        /// </summary>
        /// <param name="firearmName">The name of the firearm to which the recipe is being added.</param>
        /// <param name="cartridgeName">The name of the cartridge used in the recipe.</param>
        /// <param name="ammoDescription">A description of the ammunition used in the recipe.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Result"/> representing the outcome of the operation.</returns>
        public async Task<Result> AddNewRecipe(string firearmName, string cartridgeName, string ammoDescription,
                                               CancellationToken cancellationToken = default)
        {
            var r = await FetchStreamIdForFirearm(firearmName, cancellationToken);
            if (r.IsFailed)
            {
                return Result.Fail(r.Errors);
            }
            var e = new NewReloadingRecipeForFirearm(Guid.NewGuid(), firearmName, cartridgeName, ammoDescription, DateTimeOffset.UtcNow);

            _session.Events.Append(r.Value, e);
            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Ok();
        }

        /// <summary>
        ///     Try to get the stream ID for the firearm name.  If it doesn't exist, then create the stream.
        /// </summary>
        /// <param name="firearmName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<Guid>> FetchStreamIdForFirearm(string            firearmName,
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