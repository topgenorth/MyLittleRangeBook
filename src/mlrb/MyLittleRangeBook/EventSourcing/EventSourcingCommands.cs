using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.EventSourcing
{
    internal static class EventSourcingCommands
    {
        const string GET_EVENT_STREAM_SQL = """
                                            SELECT
                                                row_id AS RowId,
                                                id as StreamId,
                                                stream_type as StreamType,
                                                version as Version,
                                                created_utc as CreatedUtc,
                                                modified_utc as ModifiedUtc
                                            FROM event_streams
                                            WHERE id=@StreamId
                                            """;

        const string GET_EVENTS_FOR_STREAM_SQL = """
                                                 SELECT
                                                     row_id AS RowId,
                                                     id AS Id,
                                                     stream_id AS StreamId,
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
                                                 ORDER BY version;
                                                 """;

        const string SELECT_EVENTS_SQL = """
                                         select row_id as RowId,
                                                stream_id as StreamId,
                                                stream_type as StreamType,
                                                event_type as EventType,
                                                version as Version,
                                                data_json as DataJson,
                                                metadata_json as MetadataJson,
                                                occurred_utc as OccurredUtc,
                                                created_utc as Created,
                                                modified_utc as Modified
                                         from events
                                         where stream_id = @StreamId
                                         order by version;
                                         """;

        const string SELECT_STREAM_SQL = """
                                         SELECT id AS StreamId,
                                                stream_type AS StreamType,
                                                version AS Version,
                                                created_utc as Created,
                                                modified_utc as Modified
                                         FROM event_streams
                                         WHERE id = @StreamId;
                                         """;

        const string INSERT_EVENT_SQL = """
                                        insert into events
                                        (
                                            stream_id,
                                            id,
                                            stream_type,
                                            version,
                                            event_type,
                                            occurred_utc,
                                            data_json,
                                            metadata_json
                                        )
                                        values
                                        (
                                            @StreamId,
                                            @Id,
                                            @StreamType,
                                            @Version,
                                            @EventType,
                                            @OccurredUtc,
                                            @DataJson,
                                            @MetadataJson
                                        );
                                        """;

        const string UPSERT_EVENT_STREAM_SQL = """
                                               INSERT INTO event_streams (id, stream_type, version, created_utc, modified_utc)
                                               VALUES (@StreamId, @Type, @Version, utcnow(), utcnow())
                                               ON CONFLICT(id) DO UPDATE
                                               SET stream_type = excluded.stream_type,
                                                   version = excluded.version,
                                                   modified_utc = excluded.modified_utc;
                                               """;

        internal static readonly DapperCommand s_getEventStream        = new(GET_EVENT_STREAM_SQL);
        internal static readonly DapperCommand s_getEventStreamByRowId = new(GET_EVENTS_FOR_STREAM_SQL);
        internal static readonly DapperCommand s_selectEventsCommand = new(SELECT_EVENTS_SQL);
        internal static readonly DapperCommand s_selectStream        = new(SELECT_STREAM_SQL);
        internal static readonly DapperCommand s_insertEvent         = new(INSERT_EVENT_SQL);
        internal static readonly DapperCommand s_upsertEventStream   = new(UPSERT_EVENT_STREAM_SQL);
    }
}