SELECT es.Id,
       es.version,
       e.event_type,
       e.version AS EventVersion,
       e.occurred_utc AS EventDate
FROM event_streams es
    LEFT OUTER JOIN events e on e.stream_id = es.id
WHERE es.stream_type='firearm'
ORDER BY es.id, e.occurred_utc, e.version
;


SELECT s.*
FROM simple_range_events s
         LEFT JOIN firearms_simple_range_events f ON s.id = f.simple_range_event_id
WHERE f.simple_range_event_id IS NULL
ORDER BY s.firearm_name, s.id;


SELECT json_extract()
FROM events e
;

SELECT s.id  FROM simple_range_events s
;