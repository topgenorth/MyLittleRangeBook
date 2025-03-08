CREATE TABLE shotview_sessions
(
    id                TEXT    NOT NULL
        CONSTRAINT shotview_sessions_pk PRIMARY KEY,
    session_date      TEXT    NOT NULL,
    name              TEXT    NOT NULL,
    projectile_weight integer NOT NULL DEFAULT 0,
    projectile_type   TEXT    NOT NULL DEFAULT 'Rifle',
    projectile_units  TEXT    NOT NULL DEFAULT 'grains',
    velocity_units    TEXT    NOT NULL DEFAULT 'fps',
    notes             TEXT             DEFAULT NULL,
    modification_date TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    creation_date     TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX shotview_session_session_date_idx ON shotview_sessions (session_date);
CREATE UNIQUE INDEX shotview_sessions_name_uindex ON shotview_sessions (name);
CREATE INDEX shotview_session_modification_dated_idx ON shotview_sessions (modification_date);
