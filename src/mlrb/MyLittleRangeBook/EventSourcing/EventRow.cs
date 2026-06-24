using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.EventSourcing
{
    /// <summary>
    ///     A single row in the `events` table.
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
    public record struct EventRow(
        long?          RowId,
        string         Id,
        MlrbId         StreamId,
        string         StreamType,
        string         EventType,
        int            Version,
        string         DataJson,
        string         MetadataJson,
        DateTimeOffset OccurredUtc,
        DateTimeOffset CreatedUtc,
        DateTimeOffset ModifiedUtc) : IDomainEvent
    {


    };
}