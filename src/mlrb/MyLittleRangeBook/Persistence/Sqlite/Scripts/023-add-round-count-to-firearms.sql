-- Migration to add round_count column to firearms table.
ALTER TABLE firearms
    ADD COLUMN round_count INTEGER NOT NULL DEFAULT 0 CHECK (round_count >= 0);

-- noinspection SqlWithoutWhere
UPDATE firearms SET round_count = COALESCE(rounds_fired, 0);
