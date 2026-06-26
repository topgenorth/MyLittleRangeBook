CREATE TABLE events_dg_tmp
(
    row_id        INTEGER
        PRIMARY KEY AUTOINCREMENT,
    id            TEXT                              NOT NULL,
    stream_id     TEXT                              NOT NULL
        CONSTRAINT fk_events_event_streams_id
            REFERENCES event_streams (id)
            ON UPDATE RESTRICT ON DELETE RESTRICT,
    stream_type   TEXT                              NOT NULL,
    event_type    TEXT                              NOT NULL,
    version       INTEGER DEFAULT 0                 NOT NULL,
    data_json     TEXT                              NOT NULL,
    metadata_json TEXT,
    occurred_utc  TEXT                              NOT NULL,
    created_utc   TEXT    DEFAULT CURRENT_TIMESTAMP NOT NULL,
    modified_utc  TEXT    DEFAULT CURRENT_TIMESTAMP NOT NULL,
    UNIQUE (stream_id, version)
);

INSERT INTO events_dg_tmp(row_id, id, stream_id, stream_type, event_type, version, data_json, metadata_json,
                          occurred_utc, created_utc, modified_utc)
SELECT row_id,
       id,
       stream_id,
       stream_type,
       event_type,
       version,
       data_json,
       metadata_json,
       occurred_utc,
       created_utc,
       modified_utc
FROM events;

DROP TABLE events;

ALTER TABLE events_dg_tmp
    RENAME TO events;

CREATE INDEX IX_events_id_version
    ON events (id ASC, version DESC);

CREATE INDEX IX_events_stream_id_id
    ON events (stream_id, id);

CREATE INDEX IX_events_stream_type
    ON events (stream_type);

