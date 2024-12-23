CREATE TABLE shotview_sessions
(
    id TEXT NOT NULL CONSTRAINT shotview_sessions_pk PRIMARY KEY,
    session_date      TEXT    NOT NULL,
    name              integer NOT NULL,
    projectile_weight integer          DEFAULT 0 NOT NULL,
    projectile_type   TEXT    NOT NULL,
    projectile_units  TEXT    NOT NULL DEFAULT 'grains',
    velocity_units    TEXT    NOT NULL DEFAULT 'fps',
    notes             TExT             DEFAULT NULL,
    modification_date TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    creation_date     TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX shotview_session_session_date_idx ON shotview_sessions (session_date);
