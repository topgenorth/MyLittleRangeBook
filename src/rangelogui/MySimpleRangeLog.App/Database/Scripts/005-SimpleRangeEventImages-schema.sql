create table IF NOT EXISTS SimpleRangeEventImages
(
    Id                 INTEGER primary key autoincrement,
    SimpleRangeEventID INTEGER not null
        constraint SimpleRangeEventImages_SimpleRangeEvents_Id_fk
            references SimpleRangeEvents,
    FirearmId          INTEGER not null
        constraint SimpleRangeEventImages_Firearms_Id_fk
            references Firearms
);