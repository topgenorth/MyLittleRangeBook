CREATE TABLE IF NOT EXISTS Cartridges
(
    RowId    INTEGER PRIMARY KEY AUTOINCREMENT,
    Id       TEXT                           NOT NULL, --NanoID unique key.
    Name     TEXT                           NOT NULL, -- The name of the cartridge.
    CommonName TEXT,                                   -- The common name of the cartridge.
    ProjectileDiameterMetric REAL          NOT NULL, -- Projectile diameter in mm.
    ProjectileDiameterImperial REAL        NOT NULL, -- Projectile diameter in inches.
    SuitableForRifle INTEGER CHECK (SuitableForRifle IN (0, 1)) NOT NULL, -- Suitable for rifles.
    SuitableForPistol INTEGER CHECK (SuitableForPistol IN (0, 1)) NOT NULL, -- Suitable for pistols.
    Created  TEXT default CURRENT_TIMESTAMP not null, -- The date the record was created.
    Modified TEXT default CURRENT_TIMESTAMP not null, -- The date the file was last modified.
    IsActive INTEGER CHECK (IsActive IN (0, 1)) DEFAULT 1,
    CONSTRAINT UQ_Cartridges_Id UNIQUE (Id)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Cartridges_Id
    ON Cartridges (Id);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Cartridges_Name
    ON Cartridges (Name);
