-- This index will help us find range events that are associated with a firearm.
CREATE INDEX IX_events_firearm_range_assoc_stream_id_range_event_id
    ON events (
               stream_id,
               JSON_EXTRACT(data_json, '$.rangeEventId')
        )
    WHERE stream_type = 'firearm'
        AND event_type = 'range-event-associated-with-firearm';

-- This index will help us find the names of firearms that exist.
CREATE INDEX IX_events_firearm_created_stream_firearm_name
    ON events (
               stream_id,
               JSON_EXTRACT(data_json, '$.name')
        )
    WHERE stream_type = 'firearm'
        AND event_type = 'firearm-created';