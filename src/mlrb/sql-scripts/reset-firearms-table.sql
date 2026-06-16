-- Script to delete all the rows from the firearms table, and then clean out the events and events streams.

-- noinspection SqlWithoutWhere
DELETE
FROM firearms;

DELETE
FROM events
WHERE stream_type = 'firearm'
  AND id NOT IN (SELECT id FROM firearms);
DELETE
FROM event_streams
WHERE stream_type = 'firearm'
  AND id NOT IN (SELECT id FROM firearms);
DELETE
FROM event_streams
WHERE stream_type = 'firearm'
  AND id NOT IN (SELECT id FROM events);


SELECT s.firearm_name AS FirearmName,
       MIN(s.created) AS Created
FROM simple_range_events s
WHERE s.firearm_name NOT IN (SELECT name FROM firearms)
GROUP BY s.firearm_name
ORDER BY Created, s.firearm_name;

SELECT s.id           AS SimpleRangeEventId,
       s.rounds_fired AS RoundsFired,
       s.event_date   AS EventDate
FROM simple_range_events s
WHERE s.firearm_name = 'SKS'
  AND s.rounds_fired > 0
ORDER BY s.event_date;


SELECT id,
       JSON_EXTRACT(data_json, '$.sha256') AS sha256
FROM events;

-- Get a list of firearm names that already exist.
SELECT JSON_EXTRACT(data_json, '$.name') AS firearm_name
FROM events
WHERE stream_type = 'firearm'
  AND event_type = 'firearm-created';


-- Get the oldest simple range event for each firearm name that is not yet associated with a firearm.
WITH UnassociatedOldest AS (
    SELECT s.id           AS SimpleRangeEventId,
           s.firearm_name AS FirearmName,
           s.event_date   AS EventDate,
           ROW_NUMBER() OVER (PARTITION BY s.firearm_name ORDER BY s.event_date, s.id) AS rn
    FROM simple_range_events s
    WHERE NOT EXISTS (SELECT 1
                      FROM events e
                      WHERE e.stream_type = 'firearm'
                        AND e.event_type = 'firearm-created'
                        AND JSON_EXTRACT(e.data_json, '$.name') = s.firearm_name)
)
SELECT SimpleRangeEventId,
       FirearmName,
       EventDate
FROM UnassociatedOldest
WHERE rn = 1
ORDER BY EventDate, FirearmName;

-- Get the round count for all SimpleRangeEvents that are not associated with a firearm.
SELECT s.id AS SimpleRangeEventId,
       s.firearm_name AS FirearmName,
       s.rounds_fired AS RoundsFired,
       s.event_date AS EventDate
FROM simple_range_events s
WHERE s.rounds_fired > 0
ORDER BY s.event_date, s.firearm_name;