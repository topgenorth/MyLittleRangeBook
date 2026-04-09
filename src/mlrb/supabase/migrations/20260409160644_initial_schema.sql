create extension if not exists pgcrypto;

create or replace function nanoid(size integer DEFAULT 21,
                                  alphabet text DEFAULT '_-0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ'::text) returns text
    language plpgsql
as
$$
DECLARE
    idBuilder     text := '';
    i             int  := 0;
    bytes         bytea;
    alphabetIndex int;
    mask          int;
    step          int;
BEGIN
    mask := (2 << cast(floor(log(length(alphabet) - 1) / log(2)) as int)) - 1;
    step := cast(ceil(1.6 * mask * size / length(alphabet)) AS int);
    WHILE true
        LOOP
            bytes := gen_random_bytes(step);
            WHILE i < step
                LOOP
                    alphabetIndex := (get_byte(bytes, i) & mask) + 1;
                    IF alphabetIndex <= length(alphabet) THEN
                        idBuilder := idBuilder || substr(alphabet, alphabetIndex, 1);
                        IF length(idBuilder) = size THEN
                            RETURN idBuilder;
                        END IF;
                    END IF;
                    i := i + 1;
                END LOOP;
            i := 0;
        END LOOP;
END
$$;

alter function nanoid(integer, text) owner to postgres;

grant execute on function nanoid(integer, text) to anon;

grant execute on function nanoid(integer, text) to authenticated;

grant execute on function nanoid(integer, text) to service_role;

CREATE TABLE firearms
(
    row_id   BIGSERIAL PRIMARY KEY,
    id       TEXT        default nanoid() NOT NULL,
    name     TEXT                         NOT NULL,
    notes    TEXT,
    created  TIMESTAMPTZ default NOW()    not null,
    modified TIMESTAMPTZ default NOW()    not null,
    CONSTRAINT firearms_id_key UNIQUE (id)
);

COMMENT ON COLUMN firearms.id IS 'NanoID unique key.';
COMMENT ON COLUMN firearms.name IS 'The name of the firearm.';
COMMENT ON COLUMN firearms.created IS 'The date the record was created.';
COMMENT ON COLUMN firearms.modified IS 'The date the file was last modified.';

CREATE UNIQUE INDEX idx_firearms_id
    ON firearms (id);
CREATE INDEX idx_firearms_name
    ON firearms (name);


CREATE TABLE fit_files
(
    row_id    BIGSERIAL PRIMARY KEY,
    id        TEXT        default nanoid()                   not null,
    file_name TEXT                                           not null,
    mime_type TEXT        default 'application/octet-stream' not null,
    contents  BYTEA                                          not null,
    created   TIMESTAMPTZ default NOW()                      not null,
    modified  TIMESTAMPTZ default NOW()                      not null,
    CONSTRAINT fit_files_id_key UNIQUE (id)
);

COMMENT ON COLUMN fit_files.id IS 'NanoID unique key.';
COMMENT ON COLUMN fit_files.contents IS 'The file contents (bytes).';
COMMENT ON COLUMN fit_files.created IS 'The date the record was created.';
COMMENT ON COLUMN fit_files.modified IS 'The date the file was last modified.';

CREATE UNIQUE INDEX idx_fit_files_id
    ON fit_files (id);
CREATE INDEX idx_fit_files_file_name
    ON fit_files (file_name);


CREATE TABLE simple_range_events
(
    row_id           BIGSERIAL PRIMARY KEY,
    id               TEXT        default nanoid() NOT NULL,
    event_date       DATE                         NOT NULL,
    firearm_name     TEXT                         NOT NULL,
    range_name       TEXT,
    rounds_fired     INTEGER     DEFAULT 0        NOT NULL,
    ammo_description TEXT,
    notes            TEXT,
    created          TIMESTAMPTZ default NOW()    not null,
    modified         TIMESTAMPTZ default NOW()    not null,
    CONSTRAINT simple_range_events_id_key UNIQUE (id)
);

COMMENT ON COLUMN simple_range_events.id IS 'NanoID unique key.';
COMMENT ON COLUMN simple_range_events.event_date IS 'The date of the event.';
COMMENT ON COLUMN simple_range_events.firearm_name IS 'The name of the firearm. Should match the firearms table';
COMMENT ON COLUMN simple_range_events.range_name IS 'The name of the range.';
COMMENT ON COLUMN simple_range_events.rounds_fired IS 'How many rounds were fired.';
COMMENT ON COLUMN simple_range_events.created IS 'The date the record was created.';
COMMENT ON COLUMN simple_range_events.modified IS 'The date the file was last modified.';

CREATE UNIQUE INDEX idx_simple_range_events_id
    ON simple_range_events (id);
CREATE INDEX idx_simple_range_events_event_date_firearm_name_range_name
    ON simple_range_events (event_date, firearm_name, range_name);
CREATE INDEX idx_simple_range_events_firearm_date
    ON simple_range_events (firearm_name, event_date);
CREATE INDEX idx_simple_range_events_range_date
    ON simple_range_events (range_name, event_date);



