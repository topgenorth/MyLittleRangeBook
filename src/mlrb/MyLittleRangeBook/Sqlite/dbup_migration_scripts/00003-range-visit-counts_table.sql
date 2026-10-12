create table IF NOT EXISTS range_visit_counts
(
    range_name  TEXT
        primary key NOT NULL,
    visit_count INTEGER default 0 not null
);


