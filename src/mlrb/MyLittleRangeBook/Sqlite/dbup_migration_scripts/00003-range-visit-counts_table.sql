create table range_visit_counts
(
    range_name  TEXT
        primary key,
    visit_count INTEGER default 0 not null
);


