CREATE TABLE IF NOT EXISTS ShotViewFiles
(
    RowId    INTEGER PRIMARY KEY AUTOINCREMENT,
    Id       TEXT                           NOT NULL, --NanoID unique key.
    FileName TEXT                           NOT NULL,
    MimeType TEXT DEFAULT 'text/csv'        NOT NULL,
    Contents TEXT                           NOT NULL, -- The file contents as TEXT.
    Created  TEXT DEFAULT CURRENT_TIMESTAMP NOT NULL, -- The date the record was created.
    Modified TEXT DEFAULT CURRENT_TIMESTAMP NOT NULL, -- The date the file was last modified.
    CONSTRAINT UQ_ShotViewFiles_Id UNIQUE (ID),
    CONSTRAINT UQ_ShotViewFiles_FileName UNIQUE (FileName)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_ShotViewFiles_Id
    ON ShotViewFiles (Id);

CREATE TABLE IF NOT EXISTS SimpleRangeEvent_ShotViewFiles
(
    RowId              INTEGER PRIMARY KEY AUTOINCREMENT,
    SimpleRangeEventId TEXT NOT NULL, -- NanoId of the SimpleRangeEvent record.
    ShotViewFileId     TEXT NOT NULL, -- NanoId of the ShotView file record.

    CONSTRAINT FK_SimpleRangeEvent_ShotViewFiles_SimpleRangeEvents
        FOREIGN KEY (SimpleRangeEventId)
            REFERENCES SimpleRangeEvents (Id)
            ON DELETE CASCADE,

    CONSTRAINT FK_SimpleRangeEvent_ShotViewFiles_ShotViewFiles
        FOREIGN KEY (ShotViewFileId)
            REFERENCES ShotViewFiles (Id)
            ON DELETE CASCADE,

    CONSTRAINT UQ_SimpleRangeEvent_ShotViewFiles_Event_ShotViewFile
        UNIQUE (SimpleRangeEventId, ShotViewFileId)
);
