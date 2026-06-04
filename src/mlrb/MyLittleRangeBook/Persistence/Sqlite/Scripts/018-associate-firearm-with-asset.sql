DROP TABLE IF EXISTS main.SimpleRangeEvent_RangeAssetFiles;
DROP TABLE IF EXISTS main.asset_files_simple_range_events;
DROP TABLE IF EXISTS main.asset_files_firearms;
CREATE TABLE asset_files_firearms
(
    row_id                INTEGER PRIMARY KEY AUTOINCREMENT,
    firearm_id TEXT NOT NULL CONSTRAINT FK_assetfilesfirearms_firearms
            REFERENCES Firearms (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    asset_id         TEXT NOT NULL
        CONSTRAINT FK_assetfilesfirearms_assetfiles
            REFERENCES asset_files (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT UQ_assetfilesfirearms_firearmid_assetfileid
        UNIQUE (firearm_id, asset_id)
);

CREATE UNIQUE INDEX IX_assetfilesfirearms_firearmid_assetid
    ON asset_files_firearms (firearm_id, asset_id);
