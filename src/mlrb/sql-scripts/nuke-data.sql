-- Delete data from all tables except cartridges
BEGIN TRANSACTION;
DELETE FROM asset_files;
DELETE FROM asset_files_firearms;
DELETE FROM asset_files_simple_range_events;
DELETE FROM event_streams;
DELETE FROM events;
DELETE FROM notes;
DELETE FROM simple_range_events;
DELETE FROM simple_range_events_notes;
DELETE FROM fi_doc_firearm;
DELETE FROM fi_doc_firearmroundcount;
DELETE FROM fi_doc_simplerangeevent;
DELETE FROM fi_natural_key_firearm;
DELETE FROM fi_events;
DELETE FROM fi_streams;
COMMIT;

-- Vacuum the database to reclaim space
VACUUM;

-- Update index statistics
ANALYZE;