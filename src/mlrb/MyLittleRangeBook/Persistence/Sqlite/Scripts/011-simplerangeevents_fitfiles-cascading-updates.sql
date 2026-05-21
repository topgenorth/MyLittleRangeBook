CREATE TABLE SimpleRangeEvent_FitFiles_dg_tmp
(
    RowId              INTEGER
        PRIMARY KEY AUTOINCREMENT,
    SimpleRangeEventId TEXT NOT NULL
        CONSTRAINT FK_SimpleRangeEvent_FitFiles_SimpleRangeEvents
            REFERENCES SimpleRangeEvents (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    FitFileId          TEXT NOT NULL
        CONSTRAINT FK_SimpleRangeEvent_FitFiles_FitFiles
            REFERENCES FitFiles (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT UQ_SimpleRangeEvent_FitFiles_Event_FitFile
        UNIQUE (SimpleRangeEventId, FitFileId)
);

INSERT INTO SimpleRangeEvent_FitFiles_dg_tmp(RowId, SimpleRangeEventId, FitFileId)
SELECT RowId, SimpleRangeEventId, FitFileId
FROM SimpleRangeEvent_FitFiles;

DROP TABLE SimpleRangeEvent_FitFiles;

ALTER TABLE SimpleRangeEvent_FitFiles_dg_tmp
    RENAME TO SimpleRangeEvent_FitFiles;

CREATE UNIQUE INDEX IX_FitFiles_FitFileId
    ON SimpleRangeEvent_FitFiles (FitFileId);

CREATE UNIQUE INDEX IX_SimpleRangeEvent_FitFiles_FitFileId
    ON SimpleRangeEvent_FitFiles (FitFileId);

CREATE INDEX IX_SimpleRangeEvent_FitFiles_SimpleRangeEventId
    ON SimpleRangeEvent_FitFiles (SimpleRangeEventId);

