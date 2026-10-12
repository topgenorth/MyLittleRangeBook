CREATE TABLE firearm_ammo_descriptions
(
    id               TEXT NOT NULL
        CONSTRAINT pk_firearm_ammo_descriptions
            PRIMARY KEY,
    firearm_name     TEXT NOT NULL,
    ammo_description TEXT NOT NULL
);

CREATE INDEX idx_firearm_ammo_descriptions_firearm_name
    ON firearm_ammo_descriptions (firearm_name, ammo_description);

