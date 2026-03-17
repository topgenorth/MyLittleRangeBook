create table IF NOT EXISTS SimpleRangeEventImages
(
    Id                 INTEGER primary key autoincrement,
    SimpleRangeEventID INTEGER not null
        constraint SimpleRangeEventImages_SimpleRangeEvents_Id_fk
            references SimpleRangeEvents,
    ImageId            INTEGER not null
        constraint SimpleRangeEventImages_Images_Id_fk
            references Images
);