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

import java.util.HashMap;
import java.util.Map;

public class LanguageBits3  {
    public static final short BULGARIAN = 0x01;
    public static final short ROMANIAN = 0x02;
    public static final short CHINESE = 0x04;
    public static final short JAPANESE = 0x08;
    public static final short KOREAN = 0x10;
    public static final short TAIWANESE = 0x20;
    public static final short THAI = 0x40;
    public static final short HEBREW = 0x80;
    public static final short INVALID = Fit.UINT8Z_INVALID;

    private static final Map<Short, String> stringMap;

    static {
        stringMap = new HashMap<Short, String>();
        stringMap.put(BULGARIAN, "BULGARIAN");
        stringMap.put(ROMANIAN, "ROMANIAN");
        stringMap.put(CHINESE, "CHINESE");
        stringMap.put(JAPANESE, "JAPANESE");
        stringMap.put(KOREAN, "KOREAN");
        stringMap.put(TAIWANESE, "TAIWANESE");
        stringMap.put(THAI, "THAI");
        stringMap.put(HEBREW, "HEBREW");
    }


    /**
     * Retrieves the String Representation of the Value
     * @param value The enum constant
     * @return The name of this enum contsant
     */
    public static String getStringFromValue( Short value ) {
        if( stringMap.containsKey( value ) ) {
            return stringMap.get( value );
        }

        return "";
    }

    /**
     * Returns the enum constant with the specified name.
     * @param value The enum string value
     * @return The enum constant or INVALID if unknown
     */
    public static Short getValueFromString( String value ) {
        for( Map.Entry<Short, String> entry : stringMap.entrySet() ) {
            if( entry.getValue().equals( value ) ) {
                return entry.getKey();
            }
        }

        return INVALID;
    }

}
