-- It's easier to create a new table and copy data over.
CREATE TABLE IF NOT EXISTS asset_files -- Tracks any files that have been added to MLRB as an asset.
(
    row_id             INTEGER PRIMARY KEY AUTOINCREMENT,
    id                 TEXT                                    NOT NULL, -- ULID unique key.
    original_file_name TEXT                                    NOT NULL, -- The original filename of the asset file.
    path_to_asset_file TEXT                                    NULL,     -- The path to the asset file on disk. This can be NULL if the file is kept entirely in the database.
    mime_type          TEXT DEFAULT 'application/octet-stream' NOT NULL, -- The mime type of the file.
    file_content_bytes BLOB                                    NULL,     -- The file contents. Be NULL if the file is too big to keep in the database.
    created            TEXT DEFAULT CURRENT_TIMESTAMP          NOT NULL, -- The date the record was created.
    modified           TEXT DEFAULT CURRENT_TIMESTAMP          NOT NULL, -- The date the file was last modified.
    CONSTRAINT UQ_FileFiles UNIQUE (id)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_AssetFiles_Id
    ON asset_files (id);
CREATE INDEX IF NOT EXISTS IX_AssetFiles_OriginalFileName
    ON asset_files (original_file_name);
CREATE INDEX IF NOT EXISTS IX_AssetFiles_PathToAssetFile
    ON asset_files (path_to_asset_file);
CREATE INDEX IF NOT EXISTS IX_AssetFiles_MimeType
    ON asset_files (mime_type);

-- Create a table to link SimpleRangeEvents to asset_files.
CREATE TABLE IF NOT EXISTS asset_files_simple_range_events
(
    row_id              INTEGER PRIMARY KEY AUTOINCREMENT,
    simple_range_event_id TEXT NOT NULL, -- ID of the SimpleRangeEvent record.
    asset_file_id       TEXT NOT NULL, -- ID of the AssetFile record.

    CONSTRAINT FK_SimpleRangeEventAssetFiles_SimpleRangeEvents
        FOREIGN KEY (simple_range_event_id)
            REFERENCES SimpleRangeEvents (id)
            ON UPDATE CASCADE ON DELETE CASCADE,

    CONSTRAINT FK_SimpleRangeEventAssetFiles_AssetFiles
        FOREIGN KEY (asset_file_id)
            REFERENCES asset_files (id)
            ON UPDATE CASCADE ON DELETE CASCADE,

    CONSTRAINT UQ_SimpleRangeEventAssetFiles_SimpleRangeEventId_AssetFilesId
        UNIQUE (simple_range_event_id, asset_file_id)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_SimpleRangeEventAssetFiles_SimpleRangeEventId_AssetFilesId
    ON asset_files_simple_range_events (simple_range_event_id, asset_file_id);

-- Copy data over.
INSERT INTO asset_files (id, original_file_name, mime_type, file_content_bytes, path_to_asset_file, created, modified)
SELECT Id, FileName, MimeType, Contents, PathToRangeAssetFile, Created, Modified
FROM RangeAssetFiles;

INSERT INTO asset_files_simple_range_events (simple_range_event_id, asset_file_id)
SELECT SimpleRangeEventId, RangeAssetFilesId
FROM SimpleRangeEvent_RangeAssetFiles;


-- Drop the old tables.
DROP TABLE IF EXISTS SimpeleRangeEvent_AssetFiles;
DROP TABLE IF EXISTS RangeAssetFiles;
