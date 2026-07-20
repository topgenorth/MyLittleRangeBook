DROP TABLE IF EXISTS firearms_notes;
DROP TABLE IF EXISTS simple_range_event_notes;
DROP TABLE IF EXISTS asset_files_notes;
DROP TABLE IF EXISTS notes;

CREATE TABLE notes
(
    row_id       INTEGER PRIMARY KEY AUTOINCREMENT,
    id           TEXT NOT NULL,
    note_type    TEXT NOT NULL DEFAULT 'note',
    content      TEXT NOT NULL,
    created_utc  TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_utc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT UQ_notes_id UNIQUE (id)
);

CREATE INDEX IX_notes_note_type ON notes (note_type);

CREATE TABLE firearms_notes
(
    row_id     INTEGER PRIMARY KEY AUTOINCREMENT,
    firearm_id TEXT NOT NULL
        CONSTRAINT FK_firearms_notes_firearm
            REFERENCES firearms (id)
            ON UPDATE CASCADE
            ON DELETE CASCADE,
    note_id    TEXT NOT NULL
        CONSTRAINT FK_firearms_notes_note
            REFERENCES notes (id)
            ON UPDATE CASCADE
            ON DELETE CASCADE,
    CONSTRAINT UQ_firearms_notes_firearm_id_note_id UNIQUE (firearm_id, note_id)
);

CREATE TABLE simple_range_event_notes
(
    row_id                INTEGER PRIMARY KEY AUTOINCREMENT,
    simple_range_event_id TEXT NOT NULL
        CONSTRAINT FK_simple_range_event_notes_simple_range_event
            REFERENCES simple_range_events (id)
            ON UPDATE CASCADE
            ON DELETE CASCADE,
    note_id               TEXT NOT NULL
        CONSTRAINT FK_simple_range_event_notes_note
            REFERENCES notes (id)
            ON UPDATE CASCADE
            ON DELETE CASCADE,
    CONSTRAINT UQ_simple_range_event_notes_event_id_note_id UNIQUE (simple_range_event_id, note_id)
);

CREATE TABLE asset_files_notes
(
    row_id   INTEGER PRIMARY KEY AUTOINCREMENT,
    asset_id TEXT NOT NULL
        CONSTRAINT FK_asset_files_notes_asset
            REFERENCES asset_files (id)
            ON UPDATE CASCADE
            ON DELETE CASCADE,
    note_id  TEXT NOT NULL
        CONSTRAINT FK_asset_files_notes_note
            REFERENCES notes (id)
            ON UPDATE CASCADE
            ON DELETE CASCADE,
    CONSTRAINT UQ_asset_files_notes_asset_id_note_id UNIQUE (asset_id, note_id)
);
