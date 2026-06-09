UPDATE simple_range_events
SET firearm_name     = CASE
                          WHEN firearm_name IS NOT NULL
                              AND LENGTH(firearm_name) >= 2
                              AND SUBSTR(firearm_name, 1, 1) = ''
 AND SUBSTR(firearm_name, -1, 1) = ''
                              THEN SUBSTR(firearm_name, 2, LENGTH(firearm_name) - 2)
                          ELSE firearm_name
    END,
    range_name       = CASE
                          WHEN range_name IS NOT NULL
                              AND LENGTH(range_name) >= 2
                              AND SUBSTR(range_name, 1, 1) = ''
 AND SUBSTR(range_name, -1, 1) = ''
                              THEN SUBSTR(range_name, 2, LENGTH(range_name) - 2)
                          ELSE range_name
        END,
    ammo_description = CASE
                          WHEN ammo_description IS NOT NULL
                              AND LENGTH(ammo_description) >= 2
                              AND SUBSTR(ammo_description, 1, 1) = ''
 AND SUBSTR(ammo_description, -1, 1) = ''
                              THEN SUBSTR(ammo_description, 2, LENGTH(ammo_description) - 2)
                          ELSE ammo_description
        END,
    notes           = CASE
                          WHEN notes IS NOT NULL
                              AND LENGTH(notes) >= 2
                              AND SUBSTR(notes, 1, 1) = ''
 AND SUBSTR(notes, -1, 1) = ''
                              THEN SUBSTR(notes, 2, LENGTH(notes) - 2)
                          ELSE notes
        END
WHERE (firearm_name IS NOT NULL AND LENGTH(firearm_name) >= 2 AND SUBSTR(firearm_name, 1, 1) = '' AND
 SUBSTR(firearm_name, -1, 1) = '')
   OR (range_name IS NOT NULL AND LENGTH(range_name) >= 2 AND SUBSTR(range_name, 1, 1) = '' AND
 SUBSTR(range_name, -1, 1) = '')
   OR (ammo_description IS NOT NULL AND LENGTH(ammo_description) >= 2 AND SUBSTR(ammo_description, 1, 1) = '' AND
 SUBSTR(ammo_description, -1, 1) = '')
   OR (notes IS NOT NULL AND LENGTH(notes) >= 2 AND SUBSTR(notes, 1, 1) = '' AND SUBSTR(notes, -1, 1) = '');
