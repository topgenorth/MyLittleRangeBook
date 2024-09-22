/////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2024 Garmin International, Inc.
// Licensed under the Flexible and Interoperable Data Transfer (FIT) Protocol License; you
// may not use this file except in compliance with the Flexible and Interoperable Data
// Transfer (FIT) Protocol License.
/////////////////////////////////////////////////////////////////////////////////////////////
// ****WARNING****  This file is auto-generated!  Do NOT edit this file.
// Profile Version = 21.141.0Release
// Tag = production/release/21.141.0-0-g2aa27e1
/////////////////////////////////////////////////////////////////////////////////////////////


package com.garmin.fit;


public enum Goal  {
    TIME((short)0),
    DISTANCE((short)1),
    CALORIES((short)2),
    FREQUENCY((short)3),
    STEPS((short)4),
    ASCENT((short)5),
    ACTIVE_MINUTES((short)6),
    INVALID((short)255);

    protected short value;

    private Goal(short value) {
        this.value = value;
    }

    public static Goal getByValue(final Short value) {
        for (final Goal type : Goal.values()) {
            if (value == type.value)
                return type;
        }

        return Goal.INVALID;
    }

    /**
     * Retrieves the String Representation of the Value
     * @param value The enum constant
     * @return The name of this enum constant
     */
    public static String getStringFromValue( Goal value ) {
        return value.name();
    }

    public short getValue() {
        return value;
    }


}
