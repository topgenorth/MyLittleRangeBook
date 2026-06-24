namespace MyLittleRangeBook.EventSourcing
{
    /// <summary>
    /// A single row in the `event_streams` table.
    /// </summary>
    /// <param name="StreamId">A unique value that represents the event .</param>
    /// <param name="StreamType"></param>
    /// <param name="Version"></param>
    /// <param name="Created"></param>
    /// <param name="Modified"></param>
    public record struct EventStreamRow(
        string         StreamId,
        string         StreamType,
        int            Version,
        DateTimeOffset Created,
        DateTimeOffset Modified);
}