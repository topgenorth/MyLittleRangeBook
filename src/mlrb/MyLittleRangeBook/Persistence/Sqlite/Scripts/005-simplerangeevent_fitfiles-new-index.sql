DROP INDEX IX_SimpleRangeEvent_FitFiles_FitFileId;

CREATE UNIQUE INDEX IX_SimpleRangeEvent_FitFiles_FitFileId
    ON SimpleRangeEvent_FitFiles (FitFileId);