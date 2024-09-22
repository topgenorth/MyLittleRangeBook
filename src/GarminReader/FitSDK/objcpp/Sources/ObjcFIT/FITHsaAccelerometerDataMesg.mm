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


#import "FITHsaAccelerometerDataMesg.h"

@implementation FITHsaAccelerometerDataMesg

- (instancetype)init {
    self = [super initWithFitMesgIndex:fit::Profile::MESG_HSA_ACCELEROMETER_DATA];

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

// TimestampMs 
- (BOOL)isTimestampMsValid {
	const fit::Field* field = [super getField:0];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getTimestampMs {
    return ([super getFieldUINT16ValueForField:0 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setTimestampMs:(FITUInt16)timestampMs {
    [super setFieldUINT16ValueForField:0 andValue:(timestampMs) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// SamplingInterval 
- (BOOL)isSamplingIntervalValid {
	const fit::Field* field = [super getField:1];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getSamplingInterval {
    return ([super getFieldUINT16ValueForField:1 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setSamplingInterval:(FITUInt16)samplingInterval {
    [super setFieldUINT16ValueForField:1 andValue:(samplingInterval) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// AccelX 
- (uint8_t)numAccelXValues {
    return [super getFieldNumValuesForField:2 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
}

- (BOOL)isAccelXValidforIndex:(uint8_t)index {
	const fit::Field* field = [super getField:2];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid(index) == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getAccelXforIndex:(uint8_t)index {
    return ([super getFieldFLOAT32ValueForField:2 forIndex:index andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setAccelX:(FITFloat32)accelX forIndex:(uint8_t)index {
    [super setFieldFLOAT32ValueForField:2 andValue:(accelX) forIndex:index andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// AccelY 
- (uint8_t)numAccelYValues {
    return [super getFieldNumValuesForField:3 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
}

- (BOOL)isAccelYValidforIndex:(uint8_t)index {
	const fit::Field* field = [super getField:3];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid(index) == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getAccelYforIndex:(uint8_t)index {
    return ([super getFieldFLOAT32ValueForField:3 forIndex:index andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setAccelY:(FITFloat32)accelY forIndex:(uint8_t)index {
    [super setFieldFLOAT32ValueForField:3 andValue:(accelY) forIndex:index andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// AccelZ 
- (uint8_t)numAccelZValues {
    return [super getFieldNumValuesForField:4 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
}

- (BOOL)isAccelZValidforIndex:(uint8_t)index {
	const fit::Field* field = [super getField:4];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid(index) == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getAccelZforIndex:(uint8_t)index {
    return ([super getFieldFLOAT32ValueForField:4 forIndex:index andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setAccelZ:(FITFloat32)accelZ forIndex:(uint8_t)index {
    [super setFieldFLOAT32ValueForField:4 andValue:(accelZ) forIndex:index andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// Timestamp32k 
- (BOOL)isTimestamp32kValid {
	const fit::Field* field = [super getField:5];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt32)getTimestamp32k {
    return ([super getFieldUINT32ValueForField:5 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setTimestamp32k:(FITUInt32)timestamp32k {
    [super setFieldUINT32ValueForField:5 andValue:(timestamp32k) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

@end
