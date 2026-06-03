DELETE FROM main.events WHERE stream_type='mlrb-asset';
DELETE FROM main.event_streams WHERE stream_type='mlrb-asset';
DELETE FROM main.asset_files WHERE mime_type='text/csv';
