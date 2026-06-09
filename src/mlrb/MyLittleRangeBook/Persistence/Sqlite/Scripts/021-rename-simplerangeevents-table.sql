-- Migration to rename SimpleRangeEvents table and its columns to snake_case.
-- Renames table, columns, indices, and constraints.
-- Also updates referencing junction tables.

PRAGMA foreign_keys = OFF;

BEGIN TRANSACTION;

--------------------------------------------------------------------------------
-- 1. Rename SimpleRangeEvents table to simple_range_events
--------------------------------------------------------------------------------

-- Create new table simple_range_events with snake_case names
CREATE TABLE simple_range_events
(
    row_id           INTEGER PRIMARY KEY AUTOINCREMENT,
    id               TEXT                              NOT NULL, -- NanoID unique key.
    event_date       TEXT                              NOT NULL, -- The date of the event.
    firearm_name     TEXT                              NOT NULL, -- The name of the firearm.
    range_name       TEXT,                                       -- The name of the range.
    rounds_fired     INTEGER  DEFAULT 0                 NOT NULL, -- How many rounds were fired.
    ammo_description TEXT,
    notes            TEXT,
    created          TEXT    DEFAULT CURRENT_TIMESTAMP NOT NULL, -- The date the record was created.
    modified         TEXT    DEFAULT CURRENT_TIMESTAMP NOT NULL, -- The date the record was last modified.
    CONSTRAINT uq_simple_range_events_id UNIQUE (id)
);

-- Copy data from old table SimpleRangeEvents
INSERT INTO simple_range_events (row_id, id, event_date, firearm_name, range_name, rounds_fired, ammo_description, notes, created, modified)
SELECT RowId, Id, EventDate, FirearmName, RangeName, RoundsFired, AmmoDescription, Notes, Created, Modified
FROM SimpleRangeEvents;

-- Drop old table
DROP TABLE SimpleRangeEvents;

-- Create indices for simple_range_events
CREATE UNIQUE INDEX ix_simple_range_events_id
    ON simple_range_events (id);

CREATE INDEX ix_simple_range_events_event_date_firearm_name_range_name
    ON simple_range_events (event_date, firearm_name, range_name);

CREATE INDEX ix_simple_range_events_firearm_name_event_date
    ON simple_range_events (firearm_name, event_date);

CREATE INDEX ix_simple_range_events_range_name_event_date
    ON simple_range_events (range_name, event_date);

CREATE INDEX ix_simple_range_events_modified
    ON simple_range_events (modified);


--------------------------------------------------------------------------------
-- 2. Update Firearms_SimpleRangeEvents junction table
--------------------------------------------------------------------------------

-- Create new table firearms_simple_range_events
CREATE TABLE firearms_simple_range_events
(
    row_id                INTEGER PRIMARY KEY AUTOINCREMENT,
    firearm_id            TEXT NOT NULL
        CONSTRAINT fk_firearms_simple_range_events_firearms
            REFERENCES Firearms (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    simple_range_event_id TEXT NOT NULL
        CONSTRAINT fk_firearms_simple_range_events_simple_range_events
            REFERENCES simple_range_events (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT uq_firearms_simple_range_events_firearm_id_simple_range_event_id
        UNIQUE (firearm_id, simple_range_event_id)
);

-- Copy data
INSERT INTO firearms_simple_range_events (row_id, firearm_id, simple_range_event_id)
SELECT RowId, FirearmsId, SimpleRangeEventId
FROM Firearms_SimpleRangeEvents;

-- Drop old table
DROP TABLE Firearms_SimpleRangeEvents;

-- Create indices
CREATE INDEX ix_firearms_simple_range_events_firearm_id
    ON firearms_simple_range_events (firearm_id);

CREATE INDEX ix_firearms_simple_range_events_simple_range_event_id
    ON firearms_simple_range_events (simple_range_event_id);


--------------------------------------------------------------------------------
-- 3. Update asset_files_simplerangeevents junction table
--------------------------------------------------------------------------------

-- Create new table asset_files_simple_range_events
CREATE TABLE asset_files_simple_range_events
(
    row_id                INTEGER PRIMARY KEY AUTOINCREMENT,
    simple_range_event_id TEXT NOT NULL
        CONSTRAINT fk_asset_files_simple_range_events_simple_range_events
            REFERENCES simple_range_events (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    asset_id              TEXT NOT NULL
        CONSTRAINT fk_asset_files_simple_range_events_asset_files
            REFERENCES asset_files (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT uq_asset_files_simple_range_events_event_asset
        UNIQUE (simple_range_event_id, asset_id)
);

-- Copy data
INSERT INTO asset_files_simple_range_events (row_id, simple_range_event_id, asset_id)
SELECT row_id, simple_range_event_id, asset_id
FROM asset_files_simplerangeevents;

-- Drop old table
DROP TABLE asset_files_simplerangeevents;

-- Create index
CREATE UNIQUE INDEX ix_asset_files_simple_range_events_event_asset
    ON asset_files_simple_range_events (simple_range_event_id, asset_id);


COMMIT;

PRAGMA foreign_keys = ON;
