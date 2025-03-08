CREATE TABLE shotview_shots
(
    id                  TEXT                  NOT NULL
        CONSTRAINT shotview_shots_pk PRIMARY KEY,
    shotview_session_id TEXT                  NOT NULL,
    shot_number         integer               NOT NULL,
    velocity            INTEGER               NOT NULL,
    notes               TEXT    DEFAULT NULL,
    cold_bore           integer DEFAULT FALSE NOT NULL,
    clean_bore          integer DEFAULT FALSE NOT NULL,
    ignore_shot         integer DEFAULT FALSE NOT NULL,
    shot_time           TEXT                  NOT NULL,
    modification_date   TEXT                  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    creation_date       TEXT                  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (shotview_session_id) REFERENCES shotview_sessions (id)
);

CREATE UNIQUE INDEX shotview_shots_shotview_session_id_shot_number_uindex
    ON shotview_shots (shotview_session_id, shot_number);
CREATE INDEX shotview_shots_modification_date_id on shotview_shots (modification_date);
