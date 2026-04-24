ALTER TABLE Firearms ADD IsActive INTEGER CHECK (IsActive IN (0, 1)) DEFAULT 1;
UPDATE Firearms SET IsActive = 1 WHERE IsActive IS NULL;
DROP INDEX Firearms_Name_IDX;
CREATE UNIQUE INDEX Firearms_Name_IDX    ON  Firearms (Name);