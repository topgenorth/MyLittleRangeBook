-- Migration to rename Firearms table and its columns to snake_case.
-- Renames table, columns, indices, and constraints.
-- Also updates referencing junction tables.

PRAGMA foreign_keys = OFF;

BEGIN TRANSACTION;

--------------------------------------------------------------------------------
-- 1. Rename Firearms table to firearms
--------------------------------------------------------------------------------

-- Create new table firearms with snake_case names
CREATE TABLE firearms
(
    row_id       INTEGER PRIMARY KEY AUTOINCREMENT,
    id           TEXT                              NOT NULL, -- NanoID unique key.
    name         TEXT                              NOT NULL, -- The name of the firearm.
    notes        TEXT,
    is_active    INTEGER DEFAULT 1 CHECK (is_active IN (0, 1)),
    rounds_fired INTEGER DEFAULT 0,
    created      TEXT    DEFAULT CURRENT_TIMESTAMP NOT NULL, -- The date the record was created.
    modified     TEXT    DEFAULT CURRENT_TIMESTAMP NOT NULL, -- The date the record was last modified.
    CONSTRAINT uq_firearms_id UNIQUE (id),
    CONSTRAINT uq_firearms_name UNIQUE (name)
);

-- Copy data from old table Firearms
INSERT INTO firearms (row_id, id, name, notes, is_active, rounds_fired, created, modified)
SELECT RowId, Id, Name, Notes, IsActive, RoundsFired, Created, Modified
FROM Firearms;

-- Drop old table
DROP TABLE Firearms;

-- Create indices for firearms
CREATE UNIQUE INDEX ix_firearms_id
    ON firearms (id);

CREATE UNIQUE INDEX ix_firearms_name
    ON firearms (name);


--------------------------------------------------------------------------------
-- 2. Update firearms_simple_range_events junction table
--------------------------------------------------------------------------------

-- Recreate table to update foreign key reference
CREATE TABLE firearms_simple_range_events_new
(
    row_id                INTEGER PRIMARY KEY AUTOINCREMENT,
    firearm_id            TEXT NOT NULL
        CONSTRAINT fk_firearms_simple_range_events_firearms
            REFERENCES firearms (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    simple_range_event_id TEXT NOT NULL
        CONSTRAINT fk_firearms_simple_range_events_simple_range_events
            REFERENCES simple_range_events (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT uq_firearms_simple_range_events_firearm_id_simple_range_event_id
        UNIQUE (firearm_id, simple_range_event_id)
);

-- Copy data
INSERT INTO firearms_simple_range_events_new (row_id, firearm_id, simple_range_event_id)
SELECT row_id, firearm_id, simple_range_event_id
FROM firearms_simple_range_events;

-- Drop old table
DROP TABLE firearms_simple_range_events;

-- Rename new table
ALTER TABLE firearms_simple_range_events_new RENAME TO firearms_simple_range_events;

-- Create indices
CREATE INDEX ix_firearms_simple_range_events_firearm_id
    ON firearms_simple_range_events (firearm_id);

CREATE INDEX ix_firearms_simple_range_events_simple_range_event_id
    ON firearms_simple_range_events (simple_range_event_id);


--------------------------------------------------------------------------------
-- 3. Update asset_files_firearms junction table
--------------------------------------------------------------------------------

-- Recreate table to update foreign key reference
CREATE TABLE asset_files_firearms_new
(
    row_id     INTEGER PRIMARY KEY AUTOINCREMENT,
    firearm_id TEXT NOT NULL
        CONSTRAINT fk_asset_files_firearms_firearms
            REFERENCES firearms (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    asset_id   TEXT NOT NULL
        CONSTRAINT fk_asset_files_firearms_asset_files
            REFERENCES asset_files (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT uq_asset_files_firearms_firearm_id_asset_id
        UNIQUE (firearm_id, asset_id)
);

-- Copy data
INSERT INTO asset_files_firearms_new (row_id, firearm_id, asset_id)
SELECT row_id, firearm_id, asset_id
FROM asset_files_firearms;

-- Drop old table
DROP TABLE asset_files_firearms;

-- Rename new table
ALTER TABLE asset_files_firearms_new RENAME TO asset_files_firearms;

-- Create index
CREATE UNIQUE INDEX ix_asset_files_firearms_firearm_id_asset_id
    ON asset_files_firearms (firearm_id, asset_id);


COMMIT;

PRAGMA foreign_keys = ON;
