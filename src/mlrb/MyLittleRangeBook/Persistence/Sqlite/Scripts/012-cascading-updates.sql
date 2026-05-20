CREATE TABLE SimpleRangeEvent_Images_dg_tmp
(
    RowId              INTEGER
        PRIMARY KEY AUTOINCREMENT,
    SimpleRangeEventId TEXT NOT NULL
        CONSTRAINT FK_SimpleRangeEvent_Images_SimpleRangeEvents
            REFERENCES SimpleRangeEvents (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    ImageId            TEXT NOT NULL
        CONSTRAINT FK_SimpleRangeEvent_Images_RangeEventImages
            REFERENCES RangeEventImages (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT UQ_SimpleRangeEvent_Images_Event_Image
        UNIQUE (SimpleRangeEventId, ImageId)
);

INSERT INTO SimpleRangeEvent_Images_dg_tmp(RowId, SimpleRangeEventId, ImageId)
SELECT RowId, SimpleRangeEventId, ImageId
FROM SimpleRangeEvent_Images;

DROP TABLE SimpleRangeEvent_Images;

ALTER TABLE SimpleRangeEvent_Images_dg_tmp
    RENAME TO SimpleRangeEvent_Images;

CREATE INDEX IX_SimpleRangeEvent_Images_ImageId
    ON SimpleRangeEvent_Images (ImageId);

CREATE INDEX IX_SimpleRangeEvent_Images_SimpleRangeEventId
    ON SimpleRangeEvent_Images (SimpleRangeEventId);

