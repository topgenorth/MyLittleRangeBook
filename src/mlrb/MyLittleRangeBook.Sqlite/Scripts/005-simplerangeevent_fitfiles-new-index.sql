drop index IX_SimpleRangeEvent_FitFiles_FitFileId;

create unique index IX_SimpleRangeEvent_FitFiles_FitFileId
    on SimpleRangeEvent_FitFiles (FitFileId);