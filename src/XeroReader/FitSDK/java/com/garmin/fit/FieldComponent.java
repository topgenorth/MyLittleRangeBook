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

public class FieldComponent {
    protected int fieldNum;
    protected boolean accumulate;
    protected int bits;
    protected double scale;
    protected double offset;

    protected FieldComponent(int fieldNum, boolean accumulate, int bits, double scale, double offset) {
        this.fieldNum = fieldNum;
        this.accumulate = accumulate;
        this.bits = bits;
        this.scale = scale;
        this.offset = offset;
    }
}
