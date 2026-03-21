CREATE TABLE IF NOT EXISTS Firearms
(
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Name     TEXT                           NOT NULL,
    Notes    TEXT,
    Created  TEXT DEFAULT CURRENT_TIMESTAMP NOT NULL,
    Modified TEXT DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX Firearms_Name_index ON Firearms (Name);

CREATE TABLE IF NOT EXISTS SimpleRangeEventFirearms
(
    Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
    SimpleRangeEventID INTEGER NOT NULL
        CONSTRAINT SimpleRangeEventImages_SimpleRangeEvents_Id_fk
            REFERENCES SimpleRangeEvents,
    FirearmId          INTEGER NOT NULL
        CONSTRAINT SimpleRangeEventImages_Firearms_Id_fk
            REFERENCES Firearms
);

create unique index SimpleRangeEventFirearms_SimpleRangeEventID_FirearmId_uindex
    on SimpleRangeEventFirearms (SimpleRangeEventID, FirearmId);