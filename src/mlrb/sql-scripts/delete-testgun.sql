-- Helpful script to clean out stuff associated with 'Test Gun'

BEGIN TRANSACTION;

-- Cache the firearm IDs to avoid repeated subqueries
CREATE TEMP TABLE temp_firearm_ids AS
SELECT id
FROM firearms
WHERE name = 'Test Gun';

INSERT INTO temp_firearm_ids (id) VALUES ('5KR60EYQNZVCXGY505M8W9KFYD');

-- Delete in correct order respecting foreign key constraints
DELETE
FROM main.events
WHERE stream_id IN (SELECT id FROM temp_firearm_ids);

DELETE
FROM main.asset_files_firearms
WHERE firearm_id IN (SELECT id FROM temp_firearm_ids);

DELETE
FROM main.firearms_simple_range_events
WHERE firearm_id IN (SELECT id FROM temp_firearm_ids);

DELETE
FROM main.simple_range_events
WHERE firearm_name = 'Test Gun';

DELETE
FROM main.event_streams
WHERE id IN (SELECT id FROM temp_firearm_ids);

DELETE
FROM firearms
WHERE name = 'Test Gun';

-- Clean up temp table
DROP TABLE temp_firearm_ids;

COMMIT;
-- If any error occurs, SQLite will automatically ROLLBACK the transaction