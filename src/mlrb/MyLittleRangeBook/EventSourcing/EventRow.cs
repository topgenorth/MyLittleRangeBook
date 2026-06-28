using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.EventSourcing
{
    /// <summary>
    ///     A single row in the <code>events</code> table.
    /// </summary>
    /// <param name="RowId"></param>
    /// <param name="Id"></param>
    /// <param name="StreamId"></param>
    /// <param name="StreamType"></param>
    /// <param name="EventType"></param>
    /// <param name="Version"></param>
    /// <param name="DataJson"></param>
    /// <param name="MetadataJson"></param>
    /// <param name="OccurredUtc"></param>
    /// <param name="CreatedUtc"></param>
    /// <param name="ModifiedUtc"></param>
    public readonly record struct EventRow(
        long?          RowId,
        MlrbId         Id,
        MlrbId         StreamId,
        string         StreamType,
        string         EventType,
        int            Version,
        string         DataJson,
        string?        MetadataJson,
        DateTimeOffset OccurredUtc,
        DateTimeOffset CreatedUtc,
        DateTimeOffset ModifiedUtc) : IDomainEvent, IHaveMetadataJson
    {
        /// <summary>
        /// Converts the current instance of <see cref="EventRow"/> to a domain event implementation of <see cref="IDomainEvent"/>.
        /// </summary>
        /// <param name="eventSerializer">
        /// The serializer used to deserialize the event data into a domain event object.
        /// </param>
        /// <returns>
        /// An implementation of <see cref="IDomainEvent"/> that represents the current event row.
        /// </returns>
        public IDomainEvent ToDomainEvent(IEventSerializer eventSerializer)
        {
            return (IDomainEvent)eventSerializer.Deserialize(EventType, DataJson);
        }

    };
}