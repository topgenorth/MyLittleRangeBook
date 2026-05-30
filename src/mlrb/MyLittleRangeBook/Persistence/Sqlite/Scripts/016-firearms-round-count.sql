ALTER TABLE Firearms
    ADD RoundsFired INTEGER  DEFAULT 0;

-- Create a table to link SimpleRangeEvents to RangeAssetFiles.
CREATE TABLE IF NOT EXISTS Firearms_SimpleRangeEvents
(
    RowId              INTEGER PRIMARY KEY AUTOINCREMENT,
    FirearmsId         TEXT NOT NULL, -- ID of the Firearm record.
    SimpleRangeEventId TEXT NOT NULL, -- ID of the SimpleRangeEvent record.

    CONSTRAINT FK_FirearmsSimpleRangeEvent_SimpleRangeEvents
        FOREIGN KEY (SimpleRangeEventId)
            REFERENCES SimpleRangeEvents (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,

    CONSTRAINT FK_FirearmsSimpleRangeEvent_Firearms
        FOREIGN KEY (FirearmsId)
            REFERENCES Firearms (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,

    CONSTRAINT UQ_SimpleRangeEventRangeAssetFiles_SimpleRangeEventId_RangeAssetFilesId
        UNIQUE (SimpleRangeEventId, FirearmsId)
);