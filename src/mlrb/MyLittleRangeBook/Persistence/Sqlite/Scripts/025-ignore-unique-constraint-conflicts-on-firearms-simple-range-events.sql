CREATE TABLE firearms_simple_range_events_dg_tmp
(
    row_id                INTEGER
        PRIMARY KEY AUTOINCREMENT,
    firearm_id            TEXT NOT NULL
        CONSTRAINT fk_firearms_simple_range_events_firearms
            REFERENCES Firearms (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    simple_range_event_id TEXT NOT NULL
        CONSTRAINT fk_firearms_simple_range_events_simple_range_events
            REFERENCES simple_range_events (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT uq_firearms_simple_range_events_firearm_id_simple_range_event_id
        UNIQUE (firearm_id, simple_range_event_id) ON CONFLICT IGNORE
);

INSERT INTO firearms_simple_range_events_dg_tmp(row_id, firearm_id, simple_range_event_id)
SELECT row_id, firearm_id, simple_range_event_id
FROM firearms_simple_range_events;

DROP TABLE firearms_simple_range_events;

ALTER TABLE firearms_simple_range_events_dg_tmp
    RENAME TO firearms_simple_range_events;

CREATE INDEX ix_firearms_simple_range_events_firearm_id
    ON firearms_simple_range_events (firearm_id);

CREATE INDEX ix_firearms_simple_range_events_simple_range_event_id
    ON firearms_simple_range_events (simple_range_event_id);

