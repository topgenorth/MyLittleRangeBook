CREATE TABLE IF NOT EXISTS RangeEventImages
(
    RowId    INTEGER PRIMARY KEY AUTOINCREMENT,
    Id       TEXT                           not null, --NanoID unique key.
    FileName TEXT                           not null, -- Path relative to the database file.
    MimeType TEXT                           not null,
    Created  TEXT default CURRENT_TIMESTAMP not null, -- The date the record was created.
    Modified TEXT default CURRENT_TIMESTAMP not null, -- The date the file was last modified.
    CONSTRAINT UQ_RangeEventImages_Id UNIQUE (Id)
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_RangeEventImages_Id
    ON RangeEventImages (Id);

CREATE TABLE IF NOT EXISTS SimpleRangeEvent_Images
(
    RowId              INTEGER PRIMARY KEY AUTOINCREMENT,
    SimpleRangeEventId TEXT NOT NULL, -- NanoId of the SimpleRangeEvent record.
    ImageId            TEXT NOT NULL, -- NanoId of the RangeEventImages record.

    CONSTRAINT FK_SimpleRangeEvent_Images_SimpleRangeEvents
        FOREIGN KEY (SimpleRangeEventId)
            REFERENCES SimpleRangeEvents (Id)
            ON DELETE CASCADE,

    CONSTRAINT FK_SimpleRangeEvent_Images_RangeEventImages
        FOREIGN KEY (ImageId)
            REFERENCES RangeEventImages (Id)
            ON DELETE CASCADE,

    CONSTRAINT UQ_SimpleRangeEvent_Images_Event_Image
        UNIQUE (SimpleRangeEventId, ImageId)
);

CREATE INDEX IF NOT EXISTS IX_SimpleRangeEvent_Images_SimpleRangeEventId
    ON SimpleRangeEvent_Images (SimpleRangeEventId);

CREATE INDEX IF NOT EXISTS IX_SimpleRangeEvent_Images_ImageId
    ON SimpleRangeEvent_Images (ImageId);
