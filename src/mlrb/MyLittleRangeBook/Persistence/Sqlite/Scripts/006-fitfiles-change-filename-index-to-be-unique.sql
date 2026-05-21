DROP INDEX IF EXISTS FitFiles_Id_IDX;
DROP INDEX IF EXISTS IX_FitFiles_Id;
CREATE UNIQUE INDEX IX_FitFiles_Id
    ON FitFiles (Id);

DROP INDEX IF EXISTS IX_FitFiles_FitFileId;
CREATE UNIQUE INDEX IX_FitFiles_FitFileId
    ON SimpleRangeEvent_FitFiles (FitFileId);

DROP INDEX IF EXISTS FitFiles_FileName_IDX;
DROP INDEX IF EXISTS IX_FitFiles_Filename;
CREATE UNIQUE INDEX IX_FitFiles_Filename
    ON FitFiles (FileName);