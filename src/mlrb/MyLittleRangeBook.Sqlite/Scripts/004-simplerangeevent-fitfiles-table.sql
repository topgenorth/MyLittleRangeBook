CREATE TABLE IF NOT EXISTS SimpleRangeEvent_FitFiles
(
    RowId              INTEGER PRIMARY KEY AUTOINCREMENT,
    SimpleRangeEventId TEXT NOT NULL, -- NanoId of the SimpleRangeEvent record.
    FitFileId          TEXT NOT NULL, -- NanoId of the FIT file record.

    CONSTRAINT FK_SimpleRangeEvent_FitFiles_SimpleRangeEvents
        FOREIGN KEY (SimpleRangeEventId)
            REFERENCES SimpleRangeEvents (Id)
            ON DELETE CASCADE,

    CONSTRAINT FK_SimpleRangeEvent_FitFiles_FitFiles
        FOREIGN KEY (FitFileId)
            REFERENCES FitFiles (Id)
            ON DELETE CASCADE,

    CONSTRAINT UQ_SimpleRangeEvent_FitFiles_Event_FitFile
        UNIQUE (SimpleRangeEventId, FitFileId)
);

CREATE INDEX IF NOT EXISTS IX_SimpleRangeEvent_FitFiles_SimpleRangeEventId
    ON SimpleRangeEvent_FitFiles (SimpleRangeEventId);

CREATE INDEX IF NOT EXISTS IX_SimpleRangeEvent_FitFiles_FitFileId
    ON SimpleRangeEvent_FitFiles (FitFileId);