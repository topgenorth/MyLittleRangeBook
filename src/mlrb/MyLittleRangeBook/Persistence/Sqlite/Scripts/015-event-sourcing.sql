CREATE TABLE event_streams
(
    row_id       INTEGER PRIMARY KEY AUTOINCREMENT, -- A unique ID for one event stream (i.e importing a file, adding a range event, etc).
    id           TEXT    NOT NULL,                  -- The unique ID of the event stream. This is a ULID that is generated when the event stream is created. It is used to link events to the event stream.
    stream_type  TEXT    NOT NULL,                  -- The type of the event stream (e.g. "RangeEvent", "FileImport", etc).
    version      INTEGER NOT NULL DEFAULT 0,        -- The version of the event stream. The most recent version is is the hightest number.
    created_utc  TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_utc TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (id)
);
CREATE INDEX IX_event_streams_id ON event_streams (id);
CREATE INDEX IX_event_streams_stream_type_version ON event_streams (stream_type, version DESC);

CREATE TABLE events
(
    row_id        INTEGER PRIMARY KEY AUTOINCREMENT,          -- A unique ID for one event.
    id            TEXT    NOT NULL,                           -- The unique ID of the event. This is a ULID that is generated when the event is created. It is used to link events to the event stream.
    stream_id     TEXT    NOT NULL,                           -- The ID of the event stream to which the event belongs.
    stream_type   TEXT    NOT NULL,                           -- The type of the event stream to which the event belongs.
    event_type    TEXT    NOT NULL,                           -- The type of the event.
    version       INTEGER NOT NULL DEFAULT 0,                 -- The version of the event. The most recent version is is the hightest number.
    data_json     TEXT    NOT NULL,                           -- The JSON data of the event.
    metadata_json TEXT    NULL,                               -- The JSON metadata of the event. This can be NULL if there is no metadata.
    occurred_utc  TEXT    NOT NULL,                           -- The UTC timestamp when the event occurred.
    created_utc   TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP, -- The UTC timestamp when the event was created. This should never be any different than the modified_utc timestamp.
    modified_utc  TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP, -- The UTC timestamp when the event was last modified. This should never be any different than the created_utc timestamp.
    UNIQUE (stream_id, version)
);

CREATE INDEX IX_events_stream_id_id ON events (stream_id, id);
CREATE INDEX IX_events_stream_type ON events (stream_type);