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

import java.util.ArrayList;

public class SubField {
    private class SubFieldMap {
        private int refFieldNum;
        private long refFieldValue;

        protected SubFieldMap(final int refFieldNum, final long refFieldValue) {
            this.refFieldNum = refFieldNum;
            this.refFieldValue = refFieldValue;
        }

        protected boolean canMesgSupport(Mesg mesg) {
            Field field = mesg.getField(refFieldNum);

            if (field != null) {
                Long value = field.getLongValue(0, Fit.SUBFIELD_INDEX_MAIN_FIELD);
                if (value != null) {
                    if (value.longValue() == refFieldValue) {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    protected String name;
    protected int type;
    protected double scale;
    protected double offset;
    protected String units;
    private ArrayList<SubFieldMap> maps;
    protected ArrayList<FieldComponent> components;

    protected SubField(final SubField subField) {
        if (subField == null) {
            this.name = "unknown";
            this.type = 0;
            this.scale = 1;
            this.offset = 0;
            this.units = "";
            this.maps = new ArrayList<SubFieldMap>();
            this.components = new ArrayList<FieldComponent>();
            return;
        }

        this.name = subField.name;
        this.type = subField.type;
        this.scale = subField.scale;
        this.offset = subField.offset;
        this.units = subField.units;
        this.maps = subField.maps;
        this.components = subField.components;
    }

    protected SubField(String name, int type, double scale, double offset, String units) {
        this.name = name;
        this.type = type;
        this.scale = scale;
        this.offset = offset;
        this.units = units;
        this.maps = new ArrayList<SubFieldMap>();
        this.components = new ArrayList<FieldComponent>();
    }

    protected String getName() {
        return name;
    }

    protected int getType() {
        return type;
    }

    protected String getUnits() {
        return units;
    }

    protected void addMap(int refFieldNum, long refFieldValue) {
        maps.add(new SubFieldMap(refFieldNum, refFieldValue));
    }

    protected void addComponent(FieldComponent component){
        components.add(component);
    }

    public boolean canMesgSupport(Mesg mesg) {
        for (SubFieldMap map : maps) {
            if (map.canMesgSupport(mesg)) {
                return true;
            }
        }
        return false;
    }
}
