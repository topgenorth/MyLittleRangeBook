DROP TABLE IF EXISTS Cartridges;
CREATE TABLE IF NOT EXISTS cartridges
(
    row_id              INTEGER PRIMARY KEY AUTOINCREMENT,
    id                  TEXT                                                             NOT NULL, --NanoID unique key.
    name                TEXT                                                             NOT NULL, -- The name of the cartridge.
    common_name         TEXT,                                                                      -- The common name of the cartridge.
    diameter_metric     REAL CHECK (diameter_metric IS NULL OR diameter_metric >= 0)     NULL,     -- Projectile diameter in mm. If NULL, then this measured in imperial.
    diameter_imperial   REAL CHECK (diameter_imperial IS NULL OR diameter_imperial >= 0) NULL,     -- Projectile diameter in inches. IF NULL, then this is measured in metric.
    suitable_for_rifle  INTEGER CHECK (suitable_for_rifle IN (0, 1))                     NOT NULL, -- Suitable for rifles.
    suitable_for_pistol INTEGER CHECK (suitable_for_pistol IN (0, 1))                    NOT NULL, -- Suitable for pistols.
    created             TEXT                                DEFAULT CURRENT_TIMESTAMP    NOT NULL, -- The date the record was created.
    modified            TEXT                                DEFAULT CURRENT_TIMESTAMP    NOT NULL, -- The date the file was last modified.
    is_active           INTEGER CHECK (is_active IN (0, 1)) DEFAULT 1,
    CONSTRAINT UQ_cartridges_id UNIQUE (Id),
    CONSTRAINT CHK_cartridges_at_least_one_diameter
        CHECK (diameter_metric IS NOT NULL OR diameter_imperial IS NOT NULL)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_cartridges_id
    ON Cartridges (id);
CREATE UNIQUE INDEX IF NOT EXISTS IX_cartridges_name
    ON Cartridges (name);

BEGIN TRANSACTION;
INSERT INTO Cartridges (Id, Name, common_name, diameter_imperial, suitable_for_rifle,
                        suitable_for_pistol, Created, Modified, is_active)
VALUES ('7B3Z9WQFBC9VV7QBBR6DTDHSJX', '.22 Long Rifle', '.22LR',  0.22, 1, 1, '2026-05-12 03:17:41',
        '2026-05-12 03:17:41', 1);
INSERT INTO Cartridges (Id, Name, common_name, diameter_metric, diameter_imperial, suitable_for_rifle,
                        suitable_for_pistol, Created, Modified, is_active)
VALUES ('4SZ006CWE5WXBGSB9V7VFGY4H0', '.308 Winchester', '.308Win', 7.62, 0.3, 1, 1, '2026-05-12 03:20:38',
        '2026-05-12 03:20:38', 1);
INSERT INTO Cartridges (Id, Name, common_name, diameter_metric, suitable_for_rifle,
                        suitable_for_pistol, Created, Modified, is_active)
VALUES ('0AB8B8Z49GEKX2CAVR8AAGNSB2', '7.92x57', '8mm Mauser', 7.92, 1, 0, '2026-05-12 03:21:15',
        '2026-05-12 03:21:15', 1);
INSERT INTO Cartridges (Id, Name, common_name, diameter_metric, diameter_imperial, suitable_for_rifle,
                        suitable_for_pistol, Created, Modified, is_active)
VALUES ('6GQNHKKMYDHFB74NN4JJ8AD91C', '6x45mm', '6x45', 6, 0.243, 1, 0, '2026-05-12 03:23:20', '2026-05-12 03:23:20',
        1);
INSERT INTO Cartridges (Id, Name, common_name, diameter_metric, suitable_for_rifle,
                        suitable_for_pistol, Created, Modified, is_active)
VALUES ('2J6ZCWXE531BMYFPJVDEPWPPSB', '6.5x55', '6.5 Swede', 6.5, 1, 0, '2026-05-12 03:24:16',
        '2026-05-12 03:24:16', 1);
INSERT INTO Cartridges (Id, Name, common_name, diameter_metric, suitable_for_rifle,
                        suitable_for_pistol, Created, Modified, is_active)
VALUES ('1RB60C2Q1P73JG2502E9C5NJ3T', '9mm Parabellum', '9mm', 9, 1, 1, '2026-05-12 03:25:52',
        '2026-05-12 03:25:52', 1);
INSERT INTO Cartridges (Id, Name, common_name,  diameter_imperial, suitable_for_rifle,
                        suitable_for_pistol, Created, Modified, is_active)
VALUES ('3FZQB44JPBHR58R2C8NN1A78CS', '.45 ACP', '.45',  0.452, 1, 1, '2026-05-12 03:26:39',
        '2026-05-12 03:26:39', 1);
INSERT INTO Cartridges (Id, Name, common_name, diameter_metric, suitable_for_rifle,
                        suitable_for_pistol, Created, Modified, is_active)
VALUES ('1EYB5J6V589VD7P4RJ1VC86VAK', '7.62x39', '7.62x39', 7.62,  1, 0, '2026-05-12 03:27:58',
        '2026-05-12 03:27:58', 1);
INSERT INTO Cartridges (Id, Name, common_name, diameter_metric, diameter_imperial, suitable_for_rifle,
                        suitable_for_pistol, Created, Modified, is_active)
VALUES ('5SSC545R5SFRZE8DKJV7NT5X8S', '.284 Winchester', '284Win', 7, 0.284, 1, 0, '2026-05-12 03:33:08',
        '2026-05-12 03:33:08', 1);
INSERT INTO Cartridges (Id, Name, common_name, diameter_metric, diameter_imperial, suitable_for_rifle,
                        suitable_for_pistol, Created, Modified, is_active)
VALUES ('2ENWZZDST750BWCS4CJ1FBRDGW', '.223 Winchester', '0.223', 5.56, 0.224, 1, 1, '2026-05-12 03:33:28',
        '2026-05-12 03:33:28', 1);
COMMIT TRANSACTION;