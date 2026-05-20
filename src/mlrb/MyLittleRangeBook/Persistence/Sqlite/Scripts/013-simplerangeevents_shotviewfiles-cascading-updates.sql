CREATE TABLE SimpleRangeEvent_ShotViewFiles_dg_tmp
(
    RowId              INTEGER
        PRIMARY KEY AUTOINCREMENT,
    SimpleRangeEventId TEXT NOT NULL
        CONSTRAINT FK_SimpleRangeEvent_ShotViewFiles_SimpleRangeEvents
            REFERENCES SimpleRangeEvents (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    ShotViewFileId     TEXT NOT NULL
        CONSTRAINT FK_SimpleRangeEvent_ShotViewFiles_ShotViewFiles
            REFERENCES ShotViewFiles (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT UQ_SimpleRangeEvent_FitFiles_Event_FitFile
        UNIQUE (SimpleRangeEventId, ShotViewFileId)
);

INSERT INTO SimpleRangeEvent_ShotViewFiles_dg_tmp(RowId, SimpleRangeEventId, ShotViewFileId)
SELECT RowId, SimpleRangeEventId, ShotViewFileId
FROM SimpleRangeEvent_ShotViewFiles;

DROP TABLE SimpleRangeEvent_ShotViewFiles;

ALTER TABLE SimpleRangeEvent_ShotViewFiles_dg_tmp
    RENAME TO SimpleRangeEvent_ShotViewFiles;

