using Fisher;
using JasperFx.Events;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEvents
{
    public class FisherSimpleRangeEventService : ISimpleRangeEventService
    {
        readonly IDocumentSession _session;

        public FisherSimpleRangeEventService(IDocumentSession session) => _session = session;

        public async Task<Result> DeleteAsync(SimpleRangeEvent  simpleRangeEvent,
                                              CancellationToken cancellationToken = default)
        {
            _session.Delete<SimpleRangeEvent>(simpleRangeEvent);
            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Ok();
        }

        public async Task<Result<SimpleRangeEvent>> GetAsync(Guid              simpleRangeEventId,
                                                             CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public async Task<Result<Guid>> UpsertAsync(SimpleRangeEvent  sre,
                                                    CancellationToken cancellationToken = default)
        {
            _session.Store(sre);

            Guid firearmId = MlrbId.FromString(sre.FirearmName);
            Firearm? f2 = await _session.Events
                                        .AggregateStreamAsync<Firearm>(firearmId, token: cancellationToken)
                                        .ConfigureAwait(false);

            List<object> firearmEvents = [];
            if (f2 is null)
            {
                firearmEvents.Add(new FirearmCreated(sre.FirearmName, sre.OccurredUtc));
            }

            firearmEvents.Add(new FirearmActivated(sre.OccurredUtc));
            firearmEvents.Add(new FirearmAssociatedWithRangeEvent(sre.Id, sre.OccurredUtc));

            // if (sre.RoundsFired != 0)
            // {
            //     firearmEvents.Add(new Firearm.FirearmRoundCountAltered(sre.RoundsFired, sre.OccurredUtc));
            // }
            // if (!string.IsNullOrWhiteSpace(sre.AmmoDescription))
            // {
            //     firearmEvents.Add(new Firearm.FirearmUsedAmmo(sre.AmmoDescription.Trim(), sre.OccurredUtc));
            // }

            if (!string.IsNullOrWhiteSpace(sre.Notes))
            {
                firearmEvents.Add(new FirearmNoteAdded(sre.Notes.Trim(), sre.OccurredUtc));
            }

            if (!string.IsNullOrWhiteSpace(sre.RangeName))
            {
                firearmEvents.Add(new FirearmUsedAtRange(sre.RangeName.Trim(), sre.RoundsFired, sre.AmmoDescription, sre.OccurredUtc));
            }


            IEventStream<Firearm> f = await _session.Events
                                                    .FetchForWriting<Firearm>(firearmId, cancellationToken)
                                                    .ConfigureAwait(false);
            f.AppendMany(firearmEvents);

            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Ok(sre.Id);
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default)
        {

            var events = _session.Query<SimpleRangeEvent>()
                                 .OrderBy(sre => sre.EventDate)
                                 .ThenBy(sre => sre.FirearmName)
                                 .ToArray();
            return Result.Ok<IEnumerable<SimpleRangeEvent>>(events);
        }

        public async Task<Result> ExportToCsv(string csvFileName, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}