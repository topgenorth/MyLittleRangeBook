create table firearm_round_counts
(
    firearm_name TEXT              not null
        primary key,
    round_count  INTEGER default 0 not null
);

create index idx_firearm_round_counts_firearm_name
    on firearm_round_counts (firearm_name);


