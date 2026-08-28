DROP TABLE IF EXISTS main.asset_files;
DROP TABLE IF EXISTS main.asset_files_firearms;
DROP TABLE IF EXISTS main.asset_files_simple_range_events;
DROP TABLE IF EXISTS main.asset_files_notes;
DROP TABLE IF EXISTS firearms;
DROP TABLE IF EXISTS firearms_simple_range_events;
DROP TABLE IF EXISTS notes;
DROP TABLE IF EXISTS event_streams;
DROP TABLE IF EXISTS events;
DROP TABLE IF EXISTS simple_range_events;
DROP TABLE IF EXISTS simple_range_events_notes;
DROP TABLE IF EXISTS cartridges;
-- Vacuum the database to reclaim space
VACUUM;

-- Update index statistics
ANALYZE;