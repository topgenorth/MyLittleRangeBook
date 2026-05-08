UPDATE SimpleRangeEvents
SET FirearmName     = CASE
                          WHEN FirearmName IS NOT NULL
                              AND LENGTH(FirearmName) >= 2
                              AND SUBSTR(FirearmName, 1, 1) = '"'
                              AND SUBSTR(FirearmName, -1, 1) = '"'
                              THEN SUBSTR(FirearmName, 2, LENGTH(FirearmName) - 2)
                          ELSE FirearmName
    END,
    RangeName       = CASE
                          WHEN RangeName IS NOT NULL
                              AND LENGTH(RangeName) >= 2
                              AND SUBSTR(RangeName, 1, 1) = '"'
                              AND SUBSTR(RangeName, -1, 1) = '"'
                              THEN SUBSTR(RangeName, 2, LENGTH(RangeName) - 2)
                          ELSE RangeName
        END,
    AmmoDescription = CASE
                          WHEN AmmoDescription IS NOT NULL
                              AND LENGTH(AmmoDescription) >= 2
                              AND SUBSTR(AmmoDescription, 1, 1) = '"'
                              AND SUBSTR(AmmoDescription, -1, 1) = '"'
                              THEN SUBSTR(AmmoDescription, 2, LENGTH(AmmoDescription) - 2)
                          ELSE AmmoDescription
        END,
    Notes           = CASE
                          WHEN Notes IS NOT NULL
                              AND LENGTH(Notes) >= 2
                              AND SUBSTR(Notes, 1, 1) = '"'
                              AND SUBSTR(Notes, -1, 1) = '"'
                              THEN SUBSTR(Notes, 2, LENGTH(Notes) - 2)
                          ELSE Notes
        END
WHERE (FirearmName IS NOT NULL AND LENGTH(FirearmName) >= 2 AND SUBSTR(FirearmName, 1, 1) = '"' AND
       SUBSTR(FirearmName, -1, 1) = '"')
   OR (RangeName IS NOT NULL AND LENGTH(RangeName) >= 2 AND SUBSTR(RangeName, 1, 1) = '"' AND
       SUBSTR(RangeName, -1, 1) = '"')
   OR (AmmoDescription IS NOT NULL AND LENGTH(AmmoDescription) >= 2 AND SUBSTR(AmmoDescription, 1, 1) = '"' AND
       SUBSTR(AmmoDescription, -1, 1) = '"')
   OR (Notes IS NOT NULL AND LENGTH(Notes) >= 2 AND SUBSTR(Notes, 1, 1) = '"' AND SUBSTR(Notes, -1, 1) = '"');