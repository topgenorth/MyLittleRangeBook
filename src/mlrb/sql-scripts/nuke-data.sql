-- Delete data from all tables except cartridges
BEGIN TRANSACTION;
DELETE FROM fi_doc_firearm;
DELETE FROM fi_doc_firearmroundcount;
DELETE FROM fi_doc_rangevisitcount;
DELETE FROM fi_doc_simplerangeevent;
DELETE FROM fi_natural_key_firearm;
DELETE FROM fi_events;
DELETE FROM fi_streams;
DELETE FROM firearm_round_counts;
DELETE FROM range_visit_counts;
COMMIT;

-- Vacuum the database to reclaim space
VACUUM;

-- Update index statistics
ANALYZE;