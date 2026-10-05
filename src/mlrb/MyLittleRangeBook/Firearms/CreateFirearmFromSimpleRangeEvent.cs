using Fisher;
using Fisher.Exceptions;
using Fisher.Projections;
using JasperFx.Events;

namespace MyLittleRangeBook.Firearms
{
    public partial class CreateFirearmFromSimpleRangeEvent:EventProjection
    {
        public async Task Project(SimpleRangeSessionCreatedFromCli evt, IDocumentSession session)
        {
            Console.WriteLine($"---> CreateFirearmFromSimpleRangeEvent.Project called for {evt.FirearmName}");
            if (string.IsNullOrWhiteSpace(evt.FirearmName))
            {
                return;
            }

            var result = await GetStreamIdForFirearmName(session, evt.FirearmName, CancellationToken.None);
            Console.WriteLine($"---> GetStreamIdForFirearmName returned created={result.created}, id={result.firearmId}");
        }

        public async Task Project(SimpleRangeSessionCreatedFromGui evt, IDocumentSession session)
        {
            if (string.IsNullOrWhiteSpace(evt.FirearmName))
            {
                return;
            }

            await GetStreamIdForFirearmName(session, evt.FirearmName, CancellationToken.None);
        }

        async Task<(bool created, Guid firearmId)> GetStreamIdForFirearmName(IDocumentSession session, string firearmName, CancellationToken cancellationToken)
        {
            try
            {
                IEventStream<Firearm> stream =
                    await session.Events.FetchForWritingByNaturalKey<Firearm, string>(firearmName,
                        cancellationToken);
                return (false, stream.Id);
            }
            catch (UnknownNaturalKeyException)
            {
                Guid firearmId = Guid.CreateVersion7();
                FirearmCreated firearmCreated = new(firearmName, DateTimeOffset.UtcNow);
                session.Events.StartStream<Firearm>(firearmId, firearmCreated);
                return (true, firearmId);
            }
        }
    }
}