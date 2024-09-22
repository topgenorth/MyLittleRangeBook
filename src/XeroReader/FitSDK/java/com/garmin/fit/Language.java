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


public enum Language  {
    ENGLISH((short)0),
    FRENCH((short)1),
    ITALIAN((short)2),
    GERMAN((short)3),
    SPANISH((short)4),
    CROATIAN((short)5),
    CZECH((short)6),
    DANISH((short)7),
    DUTCH((short)8),
    FINNISH((short)9),
    GREEK((short)10),
    HUNGARIAN((short)11),
    NORWEGIAN((short)12),
    POLISH((short)13),
    PORTUGUESE((short)14),
    SLOVAKIAN((short)15),
    SLOVENIAN((short)16),
    SWEDISH((short)17),
    RUSSIAN((short)18),
    TURKISH((short)19),
    LATVIAN((short)20),
    UKRAINIAN((short)21),
    ARABIC((short)22),
    FARSI((short)23),
    BULGARIAN((short)24),
    ROMANIAN((short)25),
    CHINESE((short)26),
    JAPANESE((short)27),
    KOREAN((short)28),
    TAIWANESE((short)29),
    THAI((short)30),
    HEBREW((short)31),
    BRAZILIAN_PORTUGUESE((short)32),
    INDONESIAN((short)33),
    MALAYSIAN((short)34),
    VIETNAMESE((short)35),
    BURMESE((short)36),
    MONGOLIAN((short)37),
    CUSTOM((short)254),
    INVALID((short)255);

    protected short value;

    private Language(short value) {
        this.value = value;
    }

    public static Language getByValue(final Short value) {
        for (final Language type : Language.values()) {
            if (value == type.value)
                return type;
        }

        return Language.INVALID;
    }

    /**
     * Retrieves the String Representation of the Value
     * @param value The enum constant
     * @return The name of this enum constant
     */
    public static String getStringFromValue( Language value ) {
        return value.name();
    }

    public short getValue() {
        return value;
    }


}