CREATE TABLE IF NOT EXISTS SimpleRangeEvents
(
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    EventDate       TEXT                              NOT NULL,
    FirearmName     TEXT                              NOT NULL,
    RangeName       TEXT    DEFAULT 'N/A',
    RoundsFired     INTEGER DEFAULT 0                 NOT NULL,
    AmmoDescription TEXT,
    Notes           TEXT,
    Created         TEXT    DEFAULT CURRENT_TIMESTAMP NOT NULL,
    Modified        TEXT    DEFAULT CURRENT_TIMESTAMP NOT NULL
);

