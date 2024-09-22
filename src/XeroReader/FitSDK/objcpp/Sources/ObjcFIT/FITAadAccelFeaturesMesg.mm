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


#import "FITMessage+Internal.h"


#import "FITAadAccelFeaturesMesg.h"

@implementation FITAadAccelFeaturesMesg

- (instancetype)init {
    self = [super initWithFitMesgIndex:fit::Profile::MESG_AAD_ACCEL_FEATURES];

    return self;
}

// Timestamp 
- (BOOL)isTimestampValid {
	const fit::Field* field = [super getField:253];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITDate *)getTimestamp {
    return FITDateFromTimestamp([super getFieldUINT32ValueForField:253 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setTimestamp:(FITDate *)timestamp {
    [super setFieldUINT32ValueForField:253 andValue:TimestampFromFITDate(timestamp) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// Time 
- (BOOL)isTimeValid {
	const fit::Field* field = [super getField:0];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getTime {
    return ([super getFieldUINT16ValueForField:0 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setTime:(FITUInt16)time {
    [super setFieldUINT16ValueForField:0 andValue:(time) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// EnergyTotal 
- (BOOL)isEnergyTotalValid {
	const fit::Field* field = [super getField:1];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt32)getEnergyTotal {
    return ([super getFieldUINT32ValueForField:1 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setEnergyTotal:(FITUInt32)energyTotal {
    [super setFieldUINT32ValueForField:1 andValue:(energyTotal) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// ZeroCrossCnt 
- (BOOL)isZeroCrossCntValid {
	const fit::Field* field = [super getField:2];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getZeroCrossCnt {
    return ([super getFieldUINT16ValueForField:2 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setZeroCrossCnt:(FITUInt16)zeroCrossCnt {
    [super setFieldUINT16ValueForField:2 andValue:(zeroCrossCnt) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// Instance 
- (BOOL)isInstanceValid {
	const fit::Field* field = [super getField:3];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt8)getInstance {
    return ([super getFieldUINT8ValueForField:3 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setInstance:(FITUInt8)instance {
    [super setFieldUINT8ValueForField:3 andValue:(instance) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// TimeAboveThreshold 
- (BOOL)isTimeAboveThresholdValid {
	const fit::Field* field = [super getField:4];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getTimeAboveThreshold {
    return ([super getFieldFLOAT32ValueForField:4 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setTimeAboveThreshold:(FITFloat32)timeAboveThreshold {
    [super setFieldFLOAT32ValueForField:4 andValue:(timeAboveThreshold) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

@end