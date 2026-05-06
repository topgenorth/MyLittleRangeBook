drop index if exists FitFiles_Id_IDX;
DROP INDEX IF EXISTS IX_FitFiles_Id;
CREATE UNIQUE INDEX IX_FitFiles_Id
    ON FitFiles (Id);

drop index if exists IX_FitFiles_FitFileId;
create unique index IX_FitFiles_FitFileId
    on SimpleRangeEvent_FitFiles (FitFileId);

DROP INDEX IF EXISTS FitFiles_FileName_IDX;
drop index if exists IX_FitFiles_Filename;
create unique index IX_FitFiles_Filename
    on FitFiles (FileName);