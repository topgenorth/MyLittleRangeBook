-- Delete data from all tables except cartridges
DELETE
FROM asset_files;
DELETE
FROM asset_files_firearms;
DELETE
FROM asset_files_simple_range_events;
DELETE
FROM event_streams;
DELETE
FROM events;
DELETE
FROM firearms;
DELETE
FROM firearms_simple_range_events;
DELETE
FROM simple_range_events;

-- Vacuum the database to reclaim space
VACUUM;

-- Update index statistics
ANALYZE;