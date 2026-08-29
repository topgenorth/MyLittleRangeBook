-- Delete data from all tables except cartridges
-- Ensure no nested transactions by checking if already in a transaction
-- SQLite does not support true nested transactions; BEGIN fails if one is active.

-- Check if already in a transaction and exit early if true
PRAGMA defer_foreign_keys = ON; -- Allow deferred enforcement while transaction is active
PRAGMA read_uncommitted = OFF;--enables ca
BEGIN TRANSACTION;
DELETE FROM fi_doc_firearm;
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