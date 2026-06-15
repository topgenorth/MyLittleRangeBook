-- Script to delete all the rows from the firearms table, and then clean out the events and events streams.

-- noinspection SqlWithoutWhere
DELETE FROM firearms;

DELETE FROM events WHERE stream_type = 'firearm' AND id NOT IN (SELECT id FROM firearms);
DELETE FROM event_streams WHERE stream_type = 'firearm' AND id NOT IN (SELECT id FROM firearms);
DELETE FROM event_streams WHERE stream_type = 'firearm' AND id NOT IN (SELECT id FROM events);