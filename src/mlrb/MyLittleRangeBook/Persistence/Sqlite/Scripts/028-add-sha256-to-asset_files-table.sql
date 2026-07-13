ALTER TABLE asset_files
    ADD sha256 TEXT;

CREATE INDEX IX_asset_files_sha256 ON asset_files (sha256);