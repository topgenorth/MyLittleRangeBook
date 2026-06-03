DELETE FROM main.events
       WHERE stream_type<>'firearm';
DELETE FROM main.event_streams
       WHERE stream_type<>'firearm';
DELETE FROM main.asset_files;
