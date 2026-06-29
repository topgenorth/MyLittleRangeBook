using System.Reflection;
using System.Text.Json.Serialization;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.EventSourcing
{
    public interface IDomainEvent: IHaveMetadataJson
    {
        MlrbId         StreamId    { get; }
        DateTimeOffset OccurredUtc { get; }

        /// <summary>
        /// Return the value of the EventTypeAttribute.Name property if the Event has one.
        /// </summary>
        string EventType
        {
            get
            {
                Type domainEventType = GetType();

                EventTypeAttribute? eventTypeAttribute =
                    domainEventType.GetCustomAttribute<EventTypeAttribute>(false);

                if (eventTypeAttribute is not null)
                {
                    return eventTypeAttribute.Name;
                }

                string typeName = domainEventType.Name;
                return string.Concat(typeName.Select((c, i) =>
                    i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
            }
        }

    }
}