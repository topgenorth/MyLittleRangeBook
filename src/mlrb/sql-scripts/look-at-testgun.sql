BEGIN TRANSACTION;

-- Cache the firearm IDs to avoid repeated subqueries
CREATE TEMP TABLE temp_firearm_ids AS
SELECT id
FROM firearms
WHERE name LIKE 'Test Gun%';
INSERT INTO temp_firearm_ids VALUES( '5KR60EYQNZVCXGY505M8W9KFYD');

SELECT *
FROM simple_range_events
WHERE firearm_name LIKE 'Test Gun%';

SELECT *
FROM main.firearms
WHERE id IN (SELECT id FROM temp_firearm_ids);

SELECT n.*
FROM notes n
    LEFT JOIN firearms_notes fn on n.id = fn.note_id
WHERE fn.firearm_id IN (SELECT id from temp_firearm_ids)
ORDER BY n.created_utc;

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


DROP TABLE temp_firearm_ids;
ROLLBACK;