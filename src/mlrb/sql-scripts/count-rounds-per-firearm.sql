SELECT
    simple_range_events.firearm_name AS FirearmName,
    COALESCE(SUM(simple_range_events.rounds_fired), 0) AS TotalRoundsFired
FROM simple_range_events
GROUP BY simple_range_events.firearm_name
ORDER BY simple_range_events.firearm_name;