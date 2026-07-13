ALTER TABLE asset_files
    ADD sha256 TEXT NOT NULL;

CREATE UNIQUE INDEX IX_asset_files_sha256
    ON asset_files (sha256);
