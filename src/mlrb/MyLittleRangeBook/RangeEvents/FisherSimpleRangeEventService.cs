using Fisher;
using Fisher.Exceptions;
using Fisher.Linq;
using JasperFx.Events;
using MyLittleRangeBook.Firearms;

namespace MyLittleRangeBook.RangeEvents
{
    public class FisherSimpleRangeEventService : ISimpleRangeEventService
    {
        readonly ILogger          _logger;
        readonly IDocumentSession _session;

        public FisherSimpleRangeEventService(IDocumentSession session, ILogger logger)
        {
            _session = session;
            _logger  = logger;
        }

        public async Task<Result> DeleteAsync(SimpleRangeEvent  simpleRangeEvent,
                                              CancellationToken cancellationToken = default)
        {
            _session.Delete(simpleRangeEvent);
            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Ok();
        }

        public async Task<Result<SimpleRangeEvent>> GetAsync(Guid              simpleRangeEventId,
                                                             CancellationToken cancellationToken = default)
        {
            SimpleRangeEvent? sre = await _session.LoadAsync<SimpleRangeEvent>(simpleRangeEventId, cancellationToken);
            if (sre is null)
            {
                return new Result().WithError(new InvalidSimpleRangeEventIdError(simpleRangeEventId));
            }

            return Result.Ok(sre);
        }

        public async Task<Result<Guid>> UpsertAsync(SimpleRangeEvent  sre,
                                                    CancellationToken cancellationToken = default)
        {
            List<object> newEvents     = [];
            var          correlationId = sre.CorrelationId;

            (bool created, Guid firearmId) = await GetStreamIdForFirearmName(sre.FirearmName, cancellationToken);

            #region Capture the simple range event.
            SimpleRangeEventCreatedFromCommandLine e1 =
                new(DateOnly.FromDateTime(sre.EventDate),
                    sre.FirearmName,
                    Guid.CreateVersion7(),
                    correlationId,
                    correlationId,
                    sre.RangeName,
                    sre.RoundsFired,
                    sre.AmmoDescription,
                    sre.Notes,
                    DateTimeOffset.UtcNow);
            newEvents.Add(e1);
            #endregion

            if (!string.IsNullOrWhiteSpace(sre.RangeName))
            {
                // [TO20260907] Capture the range if provided.
                newEvents.Add(new FirearmUsedAtRange(sre.FirearmName,
                                                     correlationId,
                                                     e1.Id,
                                                     sre.RangeName.Trim(),
                                                     sre.RoundsFired,
                                                     sre.AmmoDescription?.Trim(),
                                                     sre.OccurredUtc));
            }

            if (sre.RoundsFired != 0)
            {
                newEvents.Add(new FirearmRoundCountAltered(sre.FirearmName,
                                                           correlationId,
                                                           e1.Id,
                                                           sre.RoundsFired,
                                                           DateTimeOffset.UtcNow));
            }

            if (!string.IsNullOrEmpty(sre.AmmoDescription))
            {
                // [TO20260907] Capture the ammo if provided.
                newEvents.Add(new FirearmUsedAmmo(sre.FirearmName,
                                                  correlationId,
                                                  e1.Id,
                                                  sre.AmmoDescription,
                                                  sre.Notes?.Trim(),
                                                  DateTimeOffset.UtcNow));
            }

            if (!string.IsNullOrWhiteSpace(sre.Notes))
            {
                newEvents.Add(new FirearmNoteAdded(sre.FirearmName,
                                                   correlationId,
                                                   e1.Id,
                                                   sre.Notes!.Trim(),
                                                   DateTimeOffset.UtcNow));
            }

            try
            {
                _session.Events.Append(firearmId, newEvents);
                _session.Store(sre);
                await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return Result.Ok(sre.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to process the simple range event `{rangeEvent}`.", sre);
                return Result.Fail($"Failed to process the simple range event `{sre}`.");
            }

        }

        /// <summary>
        ///     Returns an unsorted list of simple range event documents.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SimpleRangeEvent> events = await _session.Query<SimpleRangeEvent>()
                                                                   .ToListAsync(cancellationToken)
                                                                   .ConfigureAwait(false);
            return Result.Ok<IEnumerable<SimpleRangeEvent>>(events);
        }

        public async Task<Result> ExportToCsv(string csvFileName, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        /// <summary>
        ///     Retrieves or creates the event stream ID for the specified firearm name.
        /// </summary>
        /// <param name="sre">
        ///     The simple range event containing the firearm name for which the stream ID is required.
        /// </param>
        /// <param name="firearmName">
        ///     The name (natural key) of the firearm for which to retrieve or create a stream ID.
        /// </param>
        /// <param name="cancellationToken">
        ///     A token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        ///     A tuple indicating whether the stream was created and the associated firearm stream ID.
        /// </returns>
        async Task<(bool created, Guid firearmId)> GetStreamIdForFirearmName(string            firearmName,
                                                                             CancellationToken cancellationToken)
        {
            bool create = false;
            Guid firearmId;
            try
            {
                IEventStream<Firearm> stream =
                    await _session.Events.FetchForWritingByNaturalKey<Firearm, string>(firearmName,
                             cancellationToken);
                firearmId = stream.Id;
            }
            catch (UnknownNaturalKeyException)
            {
                _logger.Verbose("{0} is not a known natural key for a firearm.", firearmName);
                create    = true;
                firearmId = Guid.CreateVersion7();
            }

            if (!create)
            {
                return (create, firearmId);
            }


            FirearmCreated firearmCreated = new(firearmName, DateTimeOffset.UtcNow);
            StreamAction   x              = _session.Events.StartStream<Firearm>(firearmId, firearmCreated);
            _logger.Verbose("Created the stream for firearm's natural key {0}/{1}.", firearmName, firearmId);

            return (create, firearmId);
        }
    }
}