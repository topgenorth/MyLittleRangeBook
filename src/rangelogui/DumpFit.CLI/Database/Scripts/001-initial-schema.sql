CREATE TABLE IF NOT EXISTS SimpleRangeEvents
(
    RowId           INTEGER PRIMARY KEY AUTOINCREMENT,
    Id              TEXT                              NOT NULL, --NanoID unique key.
    EventDate       TEXT                              NOT NULL, -- The date of the event.
    FirearmName     TEXT                              NOT NULL, -- The name of the firearm.
    RangeName       TEXT    DEFAULT 'N/A',
    RoundsFired     INTEGER DEFAULT 0                 NOT NULL,
    AmmoDescription TEXT,
    Notes           TEXT,
    Created  TEXT default CURRENT_TIMESTAMP          not null, -- The date the record was created.
    Modified TEXT default CURRENT_TIMESTAMP          not null, -- The date the file was last modified.
    CONSTRAINT SimpleRangeEvents_Id UNIQUE (ID)
);
CREATE UNIQUE INDEX SimpleRangeEvents_Id_IDX
    ON SimpleRangeEvents (Id);
CREATE INDEX SimpleRangeEvents_EventDate_FirearmName_RangeName_IDX
    ON SimpleRangeEvents (EventDate, FirearmName, RangeName);
CREATE INDEX SimpleRangeEvents_Firearm_Date_IDX
    ON SimpleRangeEvents (FirearmName, EventDate);
CREATE INDEX SimpleRangeEvents_Range_Date_IDX
    ON SimpleRangeEvents (RangeName, EventDate);

CREATE TABLE IF NOT EXISTS FitFiles
(
    RowId    INTEGER PRIMARY KEY AUTOINCREMENT,
    Id       TEXT                                    not null, --NanoID unique key.
    FileName TEXT                                    not null,
    MimeType TEXT default 'application/octet-stream' not null,
    Contents BLOB                                    not null, -- The file contents.
    Created  TEXT default CURRENT_TIMESTAMP          not null, -- The date the record was created.
    Modified TEXT default CURRENT_TIMESTAMP          not null, -- The date the file was last modified.
    CONSTRAINT FileFiles_Id UNIQUE (ID)
);
CREATE UNIQUE INDEX FitFiles_Id_IDX
    ON FitFiles (Id);
CREATE INDEX FitFiles_FileName_IDX
    ON FitFiles (FileName);

CREATE TABLE IF NOT EXISTS Firearms
(
    RowId    INTEGER PRIMARY KEY AUTOINCREMENT,
    Id       TEXT                           NOT NULL,
    Name     TEXT                           NOT NULL,
    Notes    TEXT,
    Created  TEXT default CURRENT_TIMESTAMP not null, -- The date the record was created.
    Modified TEXT default CURRENT_TIMESTAMP not null,  -- The date the file was last modified.
    CONSTRAINT Firearms_Id UNIQUE (ID)
);
CREATE UNIQUE INDEX Firearms_Id_IDX
    ON Firearms (Id);
CREATE INDEX Firearms_Name_IDX
    ON Firearms (Name);