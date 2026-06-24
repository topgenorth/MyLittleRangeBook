using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.EventSourcing
{
    internal static class EventSourcingCommands
    {
        const string GetEventStreamSql = """
                                         SELECT
                                             row_id AS RowId,
                                             id as Id,
                                             stream_type as StreamType,
                                             version as Version,
                                             created_utc as CreatedUtc,
                                             modified_utc as ModifiedUtc
                                         FROM event_streams
                                         WHERE id=@StreamId
                                         """;

        const string GetEventsForStreamSql = """
                                             SELECT
                                                 row_id AS RowId,
                                                 id AS Id,
                                                 stream_type as StreamType,
                                                 event_type AS EventType,
                                                 version as Version,
                                                 data_json as DataJson,
                                                 metadata_json AS MetadataJson,
                                                 occurred_utc as OccuredUtc,
                                                 created_utc as CreatedUtc,
                                                 modified_utc as ModifiedUtc
                                             FROM main.events
                                             WHERE stream_id = @StreamId
                                             ORDER BY occurred_utc, version;
                                             """;

        internal static readonly DapperCommand s_getEventStream        = new(GetEventStreamSql);
        internal static readonly DapperCommand s_getEventStreamByRowId = new(GetEventsForStreamSql);
    }
}