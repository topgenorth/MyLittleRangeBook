DROP TABLE IF EXISTS main.asset_files;
DROP TABLE IF EXISTS main.asset_files_firearms;
DROP TABLE IF EXISTS main.asset_files_simple_range_events;
DROP TABLE IF EXISTS main.asset_files_notes;
DROP TABLE IF EXISTS main.firearms;
DROP TABLE IF EXISTS main.firearms_simple_range_events;
DROP TABLE IF EXISTS main.notes;
DROP TABLE IF EXISTS main.event_streams;
DROP TABLE IF EXISTS main.events;
DROP TABLE IF EXISTS main.simple_range_events;
DROP TABLE IF EXISTS main.simple_range_events_notes;
-- Vacuum the database to reclaim space
VACUUM;

-- Update index statistics
ANALYZE;