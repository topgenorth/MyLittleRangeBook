DROP TABLE IF EXISTS Cartridges;
CREATE TABLE IF NOT EXISTS cartridges
(
    row_id              INTEGER PRIMARY KEY AUTOINCREMENT,
    id                  TEXT                                                             NOT NULL, --NanoID unique key.
    name                TEXT                                                             NOT NULL, -- The name of the cartridge.
    common_name         TEXT,                                                                      -- The common name of the cartridge.
    diameter_metric     REAL CHECK (diameter_metric IS NULL OR diameter_metric >= 0)     NULL,     -- Projectile diameter in mm. If NULL, then this measured in imperial.
    diameter_imperial   REAL CHECK (diameter_imperial IS NULL OR diameter_imperial >= 0) NULL,     -- Projectile diameter in inches. IF NULL, then this is measured in metric.
    weight              REAL                                                             NULL,     -- Cartridge weight in grams.
    suitable_for_rifle  INTEGER CHECK (suitable_for_rifle IN (0, 1))                     NOT NULL, -- Suitable for rifles.
    suitable_for_pistol INTEGER CHECK (suitable_for_pistol IN (0, 1))                    NOT NULL, -- Suitable for pistols.
    created             TEXT                               DEFAULT CURRENT_TIMESTAMP     NOT NULL, -- The date the record was created.
    modified            TEXT                               DEFAULT CURRENT_TIMESTAMP     NOT NULL, -- The date the file was last modified.
    is_active           INTEGER CHECK (is_active IN (0, 1)) DEFAULT 1,
    CONSTRAINT UQ_cartridges_id UNIQUE (Id),
    CONSTRAINT CHK_cartridges_at_least_one_diameter
        CHECK (diameter_metric IS NOT NULL OR diameter_imperial IS NOT NULL)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_cartridges_id
    ON Cartridges (id);
CREATE UNIQUE INDEX IF NOT EXISTS IX_cartridges_name
    ON Cartridges (name)