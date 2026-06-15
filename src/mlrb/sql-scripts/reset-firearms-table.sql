-- Script to delete all the rows from the firearms table, and then clean out the events and events streams.

-- noinspection SqlWithoutWhere
DELETE FROM firearms;

DELETE FROM events WHERE stream_type = 'firearm' AND id NOT IN (SELECT id FROM firearms);
DELETE FROM event_streams WHERE stream_type = 'firearm' AND id NOT IN (SELECT id FROM firearms);
DELETE FROM event_streams WHERE stream_type = 'firearm' AND id NOT IN (SELECT id FROM events);


SELECT
    s.firearm_name AS FirearmName,
    MIN(s.created) AS Created
FROM simple_range_events s
WHERE s.firearm_name NOT IN (SELECT name FROM firearms)
GROUP BY s.firearm_name
ORDER BY Created, s.firearm_name;

SELECT s.id as SimpleRangeEventId,
       s.rounds_fired as RoundsFired,
       s.event_date as EventDate
FROM simple_range_events s
WHERE s.firearm_name = 'SKS' AND s.rounds_fired > 0
ORDER BY s.event_date;
