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
            List<object> newEvents = [];

            newEvents.Add(new SimpleRangeEventCreatedFromCommandLine(DateOnly.FromDateTime(sre.EventDate),
                                                                     sre.FirearmName,
                                                                     sre.RangeName,
                                                                     sre.RoundsFired,
                                                                     sre.AmmoDescription,
                                                                     sre.Notes,
                                                                     DateTimeOffset.UtcNow));

            newEvents.Add(new FirearmActivated(sre.FirearmName, DateTimeOffset.UtcNow));

            if (!string.IsNullOrWhiteSpace(sre.Notes))
            {
                newEvents.Add(new FirearmNoteAdded(sre.FirearmName, sre.Notes.Trim(), DateTimeOffset.UtcNow));
            }

            if (!string.IsNullOrWhiteSpace(sre.RangeName))
            {
                newEvents.Add(new FirearmUsedAtRange(sre.FirearmName,
                                                     sre.RangeName.Trim(),
                                                     sre.RoundsFired,
                                                     sre.AmmoDescription,
                                                     sre.OccurredUtc));
            }
            else
            {
                if (sre.RoundsFired != 0)
                {
                    newEvents.Add(new FirearmRoundCountAltered(sre.FirearmName, sre.RoundsFired,
                                                               DateTimeOffset.UtcNow));
                    if (!string.IsNullOrEmpty(sre.AmmoDescription))
                    {
                        newEvents.Add(new FirearmUsedAmmo(sre.FirearmName, sre.AmmoDescription, sre.Notes,
                                                          DateTimeOffset.UtcNow));
                    }
                }
            }

            bool create = false;
            Guid firearmId;
            try
            {
                IEventStream<Firearm> stream =
                    await _session.Events.FetchForWritingByNaturalKey<Firearm, string>(sre.FirearmName,
                        cancellationToken);
                firearmId = stream.Id;
            }
            catch (UnknownNaturalKeyException)
            {
                _logger.Verbose("{0} is not a known natural key.", sre.FirearmName);
                create    = true;
                firearmId = Guid.CreateVersion7();
            }

            if (create)
            {
                try
                {
                    StreamAction x = _session.Events.StartStream<Firearm>(firearmId,
                                                                          new FirearmCreated(sre.FirearmName,
                                                                              DateTimeOffset.UtcNow));
                    _logger.Verbose("Created the stream for natural key {0}/{1}.", sre.FirearmName, firearmId);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "An error occurred while creating the firearm stream for natural key {0}.",
                                  sre.FirearmName);
                }
            }

            _session.Events.Append(firearmId, newEvents);
            _session.Store(sre);
            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Ok(sre.Id);
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
    }
}