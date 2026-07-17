BEGIN TRANSACTION;

-- Cache the firearm IDs to avoid repeated subqueries
CREATE TEMP TABLE temp_firearm_ids AS
SELECT id
FROM firearms
WHERE name = 'Test Gun';

SELECT *
FROM simple_range_events
WHERE firearm_name = 'Test Gun';

SELECT *
FROM firearms_simple_range_events
WHERE firearm_id IN (SELECT id FROM temp_firearm_ids);

SELECT *
FROM event_streams
WHERE id IN (SELECT id FROM temp_firearm_ids);
SELECT *
FROM events
WHERE stream_id IN (SELECT id FROM temp_firearm_ids);

SELECT *
FROM asset_files_firearms
WHERE firearm_id IN (SELECT id FROM temp_firearm_ids);

SELECT *
FROM main.firearms
WHERE id IN (SELECT id FROM temp_firearm_ids);

DROP TABLE temp_firearm_ids;
ROLLBACK;