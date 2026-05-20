CREATE TABLE IF NOT EXISTS SimpleRangeEvents
(
    RowId           INTEGER PRIMARY KEY AUTOINCREMENT,
    Id              TEXT                              NOT NULL, --NanoID unique key.
    EventDate       TEXT                              NOT NULL, -- The date of the event.
    FirearmName     TEXT                              NOT NULL, -- The name of the firearm. Should match the Firearms table
    RangeName       TEXT,                                       -- The name of the range.
    RoundsFired     INTEGER DEFAULT 0                 NOT NULL, -- How many rounds were fired.
    AmmoDescription TEXT,
    Notes           TEXT,
    Created         TEXT    DEFAULT CURRENT_TIMESTAMP NOT NULL, -- The date the record was created.
    Modified        TEXT    DEFAULT CURRENT_TIMESTAMP NOT NULL, -- The date the file was last modified.
    CONSTRAINT UQ_SimpleRangeEvents_Id UNIQUE (ID)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_SimpleRangeEvents_Id
    ON SimpleRangeEvents (Id);
CREATE INDEX IF NOT EXISTS IX_SimpleRangeEvents_EventDate_FirearmName_RangeName
    ON SimpleRangeEvents (EventDate, FirearmName, RangeName);
CREATE INDEX IF NOT EXISTS IX_SimpleRangeEvents_Firearm_Date
    ON SimpleRangeEvents (FirearmName, EventDate);
CREATE INDEX IF NOT EXISTS IX_SimpleRangeEvents_Range_Date
    ON SimpleRangeEvents (RangeName, EventDate);

CREATE TABLE IF NOT EXISTS FitFiles
(
    RowId    INTEGER PRIMARY KEY AUTOINCREMENT,
    Id       TEXT                                    NOT NULL, --NanoID unique key.
    FileName TEXT                                    NOT NULL,
    MimeType TEXT DEFAULT 'application/octet-stream' NOT NULL,
    Contents BLOB                                    NOT NULL, -- The file contents.
    Created  TEXT DEFAULT CURRENT_TIMESTAMP          NOT NULL, -- The date the record was created.
    Modified TEXT DEFAULT CURRENT_TIMESTAMP          NOT NULL, -- The date the file was last modified.
    CONSTRAINT UQ_FileFiles UNIQUE (ID)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_FitFiles_Id
    ON FitFiles (Id);
CREATE INDEX IF NOT EXISTS IX_FitFiles_FileName
    ON FitFiles (FileName);

CREATE TABLE IF NOT EXISTS Firearms
(
    RowId    INTEGER PRIMARY KEY AUTOINCREMENT,
    Id       TEXT                           NOT NULL, --NanoID unique key.
    Name     TEXT                           NOT NULL, -- The name of the firearm.
    Notes    TEXT,
    Created  TEXT DEFAULT CURRENT_TIMESTAMP NOT NULL, -- The date the record was created.
    Modified TEXT DEFAULT CURRENT_TIMESTAMP NOT NULL, -- The date the file was last modified.
    CONSTRAINT UQ_Firearms_Id UNIQUE (ID)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Firearms_Id
    ON Firearms (Id);
CREATE INDEX IF NOT EXISTS IX_Firearms_Name
    ON Firearms (Name);