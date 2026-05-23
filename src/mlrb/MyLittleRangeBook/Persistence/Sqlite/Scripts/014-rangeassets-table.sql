CREATE TABLE IF NOT EXISTS RangeAssetFiles
(
    RowId                INTEGER PRIMARY KEY AUTOINCREMENT,
    Id                   TEXT                                    NOT NULL, --ULID unique key.
    FileName             TEXT                                    NOT NULL, --The filename of the
    MimeType             TEXT DEFAULT 'application/octet-stream' NOT NULL, --The mime type of the file.
    Contents             BLOB                                    NULL,     -- The file contents. Be NULL if the file is too big to keep in the database.
    PathToRangeAssetFile TEXT                                    NULL,     -- The path to the range asset file on disk. This can be NULL if the file is kept entirely in the database.
    Created              TEXT DEFAULT CURRENT_TIMESTAMP          NOT NULL, -- The date the record was created.
    Modified             TEXT DEFAULT CURRENT_TIMESTAMP          NOT NULL, -- The date the file was last modified.
    CONSTRAINT UQ_FileFiles UNIQUE (Id)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_RangeAssetFiles_Id
    ON RangeAssetFiles (Id);
CREATE INDEX IF NOT EXISTS IX_RangeAssetFiles_FileName
    ON RangeAssetFiles (FileName);
CREATE INDEX IF NOT EXISTS IX_RangeAssetFiles_PathToRangeAssetFile
    ON RangeAssetFiles (PathToRangeAssetFile);

-- Create a table to link SimpleRangeEvents to RangeAssetFiles.
CREATE TABLE IF NOT EXISTS SimpleRangeEvent_RangeAssetFiles
(
    RowId              INTEGER PRIMARY KEY AUTOINCREMENT,
    SimpleRangeEventId TEXT NOT NULL, -- ID of the SimpleRangeEvent record.
    RangeAssetFilesId  TEXT NOT NULL, -- ID of the RangeAssetFile record.

    CONSTRAINT FK_SimpleRangeEventRangeAssetFiles_SimpleRangeEvents
        FOREIGN KEY (SimpleRangeEventId)
            REFERENCES SimpleRangeEvents (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,

    CONSTRAINT FK_SimpleRangeEventRangeAssetFiles_RangeAssetFiles
        FOREIGN KEY (RangeAssetFilesId)
            REFERENCES RangeAssetFiles (Id)
            ON UPDATE CASCADE ON DELETE CASCADE,

    CONSTRAINT UQ_SimpleRangeEventRangeAssetFiles_SimpleRangeEventId_RangeAssetFilesId
        UNIQUE (SimpleRangeEventId, RangeAssetFilesId)
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_SimpleRangeEventRangeAssetFiles_SimpleRangeEventId_RangeAssetFilesId
    ON SimpleRangeEvent_RangeAssetFiles (SimpleRangeEventId, RangeAssetFilesId);


-- Copy the FitFiles to the RangeAssetFiles table. This is a one-time operation to migrate existing files to the new table. After this, the FitFiles table can be dropped if desired.
INSERT INTO RangeAssetFiles (Id, FileName, MimeType, Contents, Created, Modified)
SELECT Id, FileName, MimeType, Contents, Created, Modified
FROM FitFiles
WHERE FitFiles.Id NOT IN (SELECT Id FROM RangeAssetFiles);

INSERT INTO SimpleRangeEvent_RangeAssetFiles (SimpleRangeEventId, RangeAssetFilesId)
SELECT SimpleRangeEventId, FitFileId
FROM SimpleRangeEvent_FitFiles;


-- Drop the old tables.
DROP TABLE IF EXISTS SimpleRangeEvent_Images;
DROP TABLE IF EXISTS SimpleRangeEvent_FitFiles;
DROP TABLE IF EXISTS SimpleRangeEvent_ShotViewFiles;
DROP TABLE IF EXISTS FitFiles;
DROP TABLE IF EXISTS ShotViewFiles;
DROP TABLE IF EXISTS RangeEventImages
