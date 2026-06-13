-- First add any firearms from the simple range events that are not in the firearms table.
INSERT INTO firearms (id, name, round_count, rounds_fired)
SELECT
    LOWER(HEX(RANDOMBLOB(10))) AS id,
    s.firearm_name AS FirearmName,
    COALESCE(SUM(s.rounds_fired), 0) AS TotalRoundsFired,
    COALESCE(SUM(s.rounds_fired), 0) AS TotalRoundsFired
FROM simple_range_events s
WHERE s.firearm_name NOT IN (SELECT name FROM firearms)
GROUP BY s.firearm_name
ORDER BY s.firearm_name;

-- Associate any firearms that are in the simple range events table with the firearms table.
INSERT INTO firearms_simple_range_events (firearm_id, simple_range_event_id)
SELECT f.id, s.id
FROM simple_range_events s
         LEFT JOIN firearms f ON f.name = s.firearm_name
WHERE s.id NOT IN (SELECT simple_range_event_id FROM firearms_simple_range_events)
ORDER BY f.name;

-- Update the round count for each firearm.
UPDATE firearms
SET rounds_fired = (SELECT COALESCE(SUM(s.rounds_fired), 0)
                    FROM simple_range_events s
                    WHERE s.firearm_name = firearms.name),
    round_count  = (SELECT COALESCE(SUM(s.rounds_fired), 0)
                    FROM simple_range_events s
                    WHERE s.firearm_name = firearms.name);
