UPDATE SimpleRangeEvents
SET
    FirearmName = CASE
        WHEN FirearmName IS NOT NULL
             AND length(FirearmName) >= 2
             AND substr(FirearmName, 1, 1) = '"'
             AND substr(FirearmName, -1, 1) = '"'
            THEN substr(FirearmName, 2, length(FirearmName) - 2)
        ELSE FirearmName
    END,
    RangeName = CASE
        WHEN RangeName IS NOT NULL
             AND length(RangeName) >= 2
             AND substr(RangeName, 1, 1) = '"'
             AND substr(RangeName, -1, 1) = '"'
            THEN substr(RangeName, 2, length(RangeName) - 2)
        ELSE RangeName
    END,
    AmmoDescription = CASE
        WHEN AmmoDescription IS NOT NULL
             AND length(AmmoDescription) >= 2
             AND substr(AmmoDescription, 1, 1) = '"'
             AND substr(AmmoDescription, -1, 1) = '"'
            THEN substr(AmmoDescription, 2, length(AmmoDescription) - 2)
        ELSE AmmoDescription
    END,
    Notes = CASE
        WHEN Notes IS NOT NULL
             AND length(Notes) >= 2
             AND substr(Notes, 1, 1) = '"'
             AND substr(Notes, -1, 1) = '"'
            THEN substr(Notes, 2, length(Notes) - 2)
        ELSE Notes
    END
WHERE
    (FirearmName IS NOT NULL AND length(FirearmName) >= 2 AND substr(FirearmName, 1, 1) = '"' AND substr(FirearmName, -1, 1) = '"')
    OR (RangeName IS NOT NULL AND length(RangeName) >= 2 AND substr(RangeName, 1, 1) = '"' AND substr(RangeName, -1, 1) = '"')
    OR (AmmoDescription IS NOT NULL AND length(AmmoDescription) >= 2 AND substr(AmmoDescription, 1, 1) = '"' AND substr(AmmoDescription, -1, 1) = '"')
    OR (Notes IS NOT NULL AND length(Notes) >= 2 AND substr(Notes, 1, 1) = '"' AND substr(Notes, -1, 1) = '"');