INSERT INTO Firearms (Id, Name, Notes)
VALUES (lower(hex(randomblob(8))), 'Test', 'Just a test record - ignore / delete.');

INSERT INTO main.SimpleRangeEvents (Id, EventDate, FirearmName, RangeName, RoundsFired, AmmoDescription, Notes)
VALUES (lower(hex(randomblob(8))), '2019-01-01', 'Test', 'Test', 1, 'Test', 'Just a test record - ignore / delete.');

INSERT INTO FitFiles (Id, FileName, Contents)
VALUES (lower(hex(randomblob(8))), 'test.sql', randomblob(1000));