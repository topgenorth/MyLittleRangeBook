using System.Text.Json.Nodes;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;

namespace MyLittleRangeBook.RangeEvents
{
    public class SimpleRangeEventProcessor
    {

        public IEnumerable<IDomainEvent> ToFirearmDomainEvents(IEnumerable<SimpleRangeEvent> rangeEvents)
        {
            return rangeEvents.SelectMany(ToFirearmDomainEvents);
        }


        public IEnumerable<IDomainEvent> ToFirearmDomainEvents(SimpleRangeEvent sre)
        {
            DateTimeOffset occuredUtc = sre.EventDate.Kind == DateTimeKind.Utc
                                            ? new DateTimeOffset(sre.EventDate)
                                            : new DateTimeOffset(DateTime.SpecifyKind(sre.EventDate,
                                                                     DateTimeKind.Local)).ToUniversalTime();

            string metaDataJson =  new JsonObject { ["simple_range_event_id"] = sre.Id! }.ToJsonString();


            FirearmAggregate fa = FirearmAggregate.New(sre.FirearmName, occuredUtc);
            fa.AssociateWithSimpleRangeEvent(sre.Id!, occuredUtc);
            fa.MoreRoundsFired(sre.RoundsFired, occuredUtc, metaDataJson);

            if (!string.IsNullOrWhiteSpace(sre.Notes))
            {
                fa.AppendToNotes(sre.Notes, occuredUtc, metaDataJson);
            }

            if (!string.IsNullOrWhiteSpace(sre.AmmoDescription))
            {
                fa.AppendToNotes(sre.AmmoDescription, occuredUtc, metaDataJson);
            }

            IEnumerable<IDomainEvent> events =
                fa.DequeueUncommittedEvents().Where(e => e is not FirearmAggregate.FirearmCreated);
            return events;
        }
    }
}