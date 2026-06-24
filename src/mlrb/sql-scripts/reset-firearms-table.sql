-- Script to delete all the rows from the firearms table, and then clean out the events and events streams.

DELETE
FROM firearms
WHERE id IN ('5KR60EYQNZVCXGY505M8W9KFYD', '7GX9MA4B0GX469ZXNZ8TM01AET');

DELETE
FROM events
WHERE stream_type = 'firearm'
  AND stream_id IN ('5KR60EYQNZVCXGY505M8W9KFYD', '7GX9MA4B0GX469ZXNZ8TM01AET');

DELETE
FROM event_streams
WHERE stream_type = 'firearm'
  AND id IN ('5KR60EYQNZVCXGY505M8W9KFYD', '7GX9MA4B0GX469ZXNZ8TM01AET');

DELETE FROM firearms_simple_range_events
WHERE firearm_id IN ('5KR60EYQNZVCXGY505M8W9KFYD', '7GX9MA4B0GX469ZXNZ8TM01AET');
