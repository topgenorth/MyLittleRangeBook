DROP TABLE IF EXISTS main.asset_files_simplerangeevents;
CREATE TABLE asset_files_simplerangeevents
(
    row_id                INTEGER PRIMARY KEY AUTOINCREMENT,
    simple_range_event_id TEXT NOT NULL CONSTRAINT FK_assetfilessimplerangeevents_simplerangeevents
        REFERENCES SimpleRangeEvents (id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    asset_id         TEXT NOT NULL
        CONSTRAINT FK_assetfilessimplerangeevents_assetfiles
            REFERENCES asset_files (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT UQ_assetfilessimplerangeevents_firearmid_assetfileid
        UNIQUE (simple_range_event_id, asset_id)
);

CREATE UNIQUE INDEX IX_assetfilessimplerangeevents_firearmid_assetid
    ON asset_files_simplerangeevents (simple_range_event_id, asset_id);
