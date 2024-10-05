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


#import "FITEventMesg.h"

@implementation FITEventMesg

- (instancetype)init {
    self = [super initWithFitMesgIndex:fit::Profile::MESG_EVENT];

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

// Event 
- (BOOL)isEventValid {
	const fit::Field* field = [super getField:0];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITEvent)getEvent {
    return ([super getFieldENUMValueForField:0 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setEvent:(FITEvent)event {
    [super setFieldENUMValueForField:0 andValue:(event) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// EventType 
- (BOOL)isEventTypeValid {
	const fit::Field* field = [super getField:1];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITEventType)getEventType {
    return ([super getFieldENUMValueForField:1 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setEventType:(FITEventType)eventType {
    [super setFieldENUMValueForField:1 andValue:(eventType) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// Data16 
- (BOOL)isData16Valid {
	const fit::Field* field = [super getField:2];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getData16 {
    return ([super getFieldUINT16ValueForField:2 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setData16:(FITUInt16)data16 {
    [super setFieldUINT16ValueForField:2 andValue:(data16) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// Data 
- (BOOL)isDataValid {
	const fit::Field* field = [super getField:3];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt32)getData {
    return ([super getFieldUINT32ValueForField:3 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setData:(FITUInt32)data {
    [super setFieldUINT32ValueForField:3 andValue:(data) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 
// Data - Sub Fields
// TimerTrigger - Data Field - Sub Field 
- (BOOL)isTimerTriggerValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldTimerTriggerSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldTimerTriggerSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITTimerTrigger)getTimerTrigger
{
    return ([super getFieldENUMValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldTimerTriggerSubField]);
}

- (void)setTimerTrigger:(FITTimerTrigger)timerTrigger
{
    [super setFieldENUMValueForField:3 andValue:(timerTrigger) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldTimerTriggerSubField];
} 
// CoursePointIndex - Data Field - Sub Field 
- (BOOL)isCoursePointIndexValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldCoursePointIndexSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldCoursePointIndexSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITMessageIndex)getCoursePointIndex
{
    return ([super getFieldUINT16ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldCoursePointIndexSubField]);
}

- (void)setCoursePointIndex:(FITMessageIndex)coursePointIndex
{
    [super setFieldUINT16ValueForField:3 andValue:(coursePointIndex) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldCoursePointIndexSubField];
} 
// BatteryLevel - Data Field - Sub Field 
- (BOOL)isBatteryLevelValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldBatteryLevelSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldBatteryLevelSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getBatteryLevel
{
    return ([super getFieldFLOAT32ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldBatteryLevelSubField]);
}

- (void)setBatteryLevel:(FITFloat32)batteryLevel
{
    [super setFieldFLOAT32ValueForField:3 andValue:(batteryLevel) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldBatteryLevelSubField];
} 
// VirtualPartnerSpeed - Data Field - Sub Field 
- (BOOL)isVirtualPartnerSpeedValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldVirtualPartnerSpeedSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldVirtualPartnerSpeedSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getVirtualPartnerSpeed
{
    return ([super getFieldFLOAT32ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldVirtualPartnerSpeedSubField]);
}

- (void)setVirtualPartnerSpeed:(FITFloat32)virtualPartnerSpeed
{
    [super setFieldFLOAT32ValueForField:3 andValue:(virtualPartnerSpeed) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldVirtualPartnerSpeedSubField];
} 
// HrHighAlert - Data Field - Sub Field 
- (BOOL)isHrHighAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldHrHighAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldHrHighAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt8)getHrHighAlert
{
    return ([super getFieldUINT8ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldHrHighAlertSubField]);
}

- (void)setHrHighAlert:(FITUInt8)hrHighAlert
{
    [super setFieldUINT8ValueForField:3 andValue:(hrHighAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldHrHighAlertSubField];
} 
// HrLowAlert - Data Field - Sub Field 
- (BOOL)isHrLowAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldHrLowAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldHrLowAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt8)getHrLowAlert
{
    return ([super getFieldUINT8ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldHrLowAlertSubField]);
}

- (void)setHrLowAlert:(FITUInt8)hrLowAlert
{
    [super setFieldUINT8ValueForField:3 andValue:(hrLowAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldHrLowAlertSubField];
} 
// SpeedHighAlert - Data Field - Sub Field 
- (BOOL)isSpeedHighAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldSpeedHighAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldSpeedHighAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getSpeedHighAlert
{
    return ([super getFieldFLOAT32ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldSpeedHighAlertSubField]);
}

- (void)setSpeedHighAlert:(FITFloat32)speedHighAlert
{
    [super setFieldFLOAT32ValueForField:3 andValue:(speedHighAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldSpeedHighAlertSubField];
} 
// SpeedLowAlert - Data Field - Sub Field 
- (BOOL)isSpeedLowAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldSpeedLowAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldSpeedLowAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getSpeedLowAlert
{
    return ([super getFieldFLOAT32ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldSpeedLowAlertSubField]);
}

- (void)setSpeedLowAlert:(FITFloat32)speedLowAlert
{
    [super setFieldFLOAT32ValueForField:3 andValue:(speedLowAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldSpeedLowAlertSubField];
} 
// CadHighAlert - Data Field - Sub Field 
- (BOOL)isCadHighAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldCadHighAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldCadHighAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getCadHighAlert
{
    return ([super getFieldUINT16ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldCadHighAlertSubField]);
}

- (void)setCadHighAlert:(FITUInt16)cadHighAlert
{
    [super setFieldUINT16ValueForField:3 andValue:(cadHighAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldCadHighAlertSubField];
} 
// CadLowAlert - Data Field - Sub Field 
- (BOOL)isCadLowAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldCadLowAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldCadLowAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getCadLowAlert
{
    return ([super getFieldUINT16ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldCadLowAlertSubField]);
}

- (void)setCadLowAlert:(FITUInt16)cadLowAlert
{
    [super setFieldUINT16ValueForField:3 andValue:(cadLowAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldCadLowAlertSubField];
} 
// PowerHighAlert - Data Field - Sub Field 
- (BOOL)isPowerHighAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldPowerHighAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldPowerHighAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getPowerHighAlert
{
    return ([super getFieldUINT16ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldPowerHighAlertSubField]);
}

- (void)setPowerHighAlert:(FITUInt16)powerHighAlert
{
    [super setFieldUINT16ValueForField:3 andValue:(powerHighAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldPowerHighAlertSubField];
} 
// PowerLowAlert - Data Field - Sub Field 
- (BOOL)isPowerLowAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldPowerLowAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldPowerLowAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getPowerLowAlert
{
    return ([super getFieldUINT16ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldPowerLowAlertSubField]);
}

- (void)setPowerLowAlert:(FITUInt16)powerLowAlert
{
    [super setFieldUINT16ValueForField:3 andValue:(powerLowAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldPowerLowAlertSubField];
} 
// TimeDurationAlert - Data Field - Sub Field 
- (BOOL)isTimeDurationAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldTimeDurationAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldTimeDurationAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getTimeDurationAlert
{
    return ([super getFieldFLOAT32ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldTimeDurationAlertSubField]);
}

- (void)setTimeDurationAlert:(FITFloat32)timeDurationAlert
{
    [super setFieldFLOAT32ValueForField:3 andValue:(timeDurationAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldTimeDurationAlertSubField];
} 
// DistanceDurationAlert - Data Field - Sub Field 
- (BOOL)isDistanceDurationAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldDistanceDurationAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldDistanceDurationAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getDistanceDurationAlert
{
    return ([super getFieldFLOAT32ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldDistanceDurationAlertSubField]);
}

- (void)setDistanceDurationAlert:(FITFloat32)distanceDurationAlert
{
    [super setFieldFLOAT32ValueForField:3 andValue:(distanceDurationAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldDistanceDurationAlertSubField];
} 
// CalorieDurationAlert - Data Field - Sub Field 
- (BOOL)isCalorieDurationAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldCalorieDurationAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldCalorieDurationAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt32)getCalorieDurationAlert
{
    return ([super getFieldUINT32ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldCalorieDurationAlertSubField]);
}

- (void)setCalorieDurationAlert:(FITUInt32)calorieDurationAlert
{
    [super setFieldUINT32ValueForField:3 andValue:(calorieDurationAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldCalorieDurationAlertSubField];
} 
// FitnessEquipmentState - Data Field - Sub Field 
- (BOOL)isFitnessEquipmentStateValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldFitnessEquipmentStateSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldFitnessEquipmentStateSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITFitnessEquipmentState)getFitnessEquipmentState
{
    return ([super getFieldENUMValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldFitnessEquipmentStateSubField]);
}

- (void)setFitnessEquipmentState:(FITFitnessEquipmentState)fitnessEquipmentState
{
    [super setFieldENUMValueForField:3 andValue:(fitnessEquipmentState) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldFitnessEquipmentStateSubField];
} 
// SportPoint - Data Field - Sub Field 
- (BOOL)isSportPointValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldSportPointSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldSportPointSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt32)getSportPoint
{
    return ([super getFieldUINT32ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldSportPointSubField]);
}

- (void)setSportPoint:(FITUInt32)sportPoint
{
    [super setFieldUINT32ValueForField:3 andValue:(sportPoint) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldSportPointSubField];
} 
// GearChangeData - Data Field - Sub Field 
- (BOOL)isGearChangeDataValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldGearChangeDataSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldGearChangeDataSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt32)getGearChangeData
{
    return ([super getFieldUINT32ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldGearChangeDataSubField]);
}

- (void)setGearChangeData:(FITUInt32)gearChangeData
{
    [super setFieldUINT32ValueForField:3 andValue:(gearChangeData) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldGearChangeDataSubField];
} 
// RiderPosition - Data Field - Sub Field 
- (BOOL)isRiderPositionValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldRiderPositionSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldRiderPositionSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITRiderPositionType)getRiderPosition
{
    return ([super getFieldENUMValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldRiderPositionSubField]);
}

- (void)setRiderPosition:(FITRiderPositionType)riderPosition
{
    [super setFieldENUMValueForField:3 andValue:(riderPosition) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldRiderPositionSubField];
} 
// CommTimeout - Data Field - Sub Field 
- (BOOL)isCommTimeoutValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldCommTimeoutSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldCommTimeoutSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITCommTimeoutType)getCommTimeout
{
    return ([super getFieldUINT16ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldCommTimeoutSubField]);
}

- (void)setCommTimeout:(FITCommTimeoutType)commTimeout
{
    [super setFieldUINT16ValueForField:3 andValue:(commTimeout) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldCommTimeoutSubField];
} 
// DiveAlert - Data Field - Sub Field 
- (BOOL)isDiveAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldDiveAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldDiveAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITDiveAlert)getDiveAlert
{
    return ([super getFieldENUMValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldDiveAlertSubField]);
}

- (void)setDiveAlert:(FITDiveAlert)diveAlert
{
    [super setFieldENUMValueForField:3 andValue:(diveAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldDiveAlertSubField];
} 
// AutoActivityDetectDuration - Data Field - Sub Field 
- (BOOL)isAutoActivityDetectDurationValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldAutoActivityDetectDurationSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldAutoActivityDetectDurationSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getAutoActivityDetectDuration
{
    return ([super getFieldUINT16ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldAutoActivityDetectDurationSubField]);
}

- (void)setAutoActivityDetectDuration:(FITUInt16)autoActivityDetectDuration
{
    [super setFieldUINT16ValueForField:3 andValue:(autoActivityDetectDuration) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldAutoActivityDetectDurationSubField];
} 
// RadarThreatAlert - Data Field - Sub Field 
- (BOOL)isRadarThreatAlertValid
{
    const fit::Field* field = [super getField:3];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:3 supportSubField:(FITUInt16)FITEventMesgDataFieldRadarThreatAlertSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgDataFieldRadarThreatAlertSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt32)getRadarThreatAlert
{
    return ([super getFieldUINT32ValueForField:3 forIndex:0 andSubFieldIndex:FITEventMesgDataFieldRadarThreatAlertSubField]);
}

- (void)setRadarThreatAlert:(FITUInt32)radarThreatAlert
{
    [super setFieldUINT32ValueForField:3 andValue:(radarThreatAlert) forIndex:0 andSubFieldIndex:FITEventMesgDataFieldRadarThreatAlertSubField];
} 

// EventGroup 
- (BOOL)isEventGroupValid {
	const fit::Field* field = [super getField:4];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt8)getEventGroup {
    return ([super getFieldUINT8ValueForField:4 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setEventGroup:(FITUInt8)eventGroup {
    [super setFieldUINT8ValueForField:4 andValue:(eventGroup) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// Score 
- (BOOL)isScoreValid {
	const fit::Field* field = [super getField:7];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getScore {
    return ([super getFieldUINT16ValueForField:7 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setScore:(FITUInt16)score {
    [super setFieldUINT16ValueForField:7 andValue:(score) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// OpponentScore 
- (BOOL)isOpponentScoreValid {
	const fit::Field* field = [super getField:8];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt16)getOpponentScore {
    return ([super getFieldUINT16ValueForField:8 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setOpponentScore:(FITUInt16)opponentScore {
    [super setFieldUINT16ValueForField:8 andValue:(opponentScore) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// FrontGearNum 
- (BOOL)isFrontGearNumValid {
	const fit::Field* field = [super getField:9];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt8z)getFrontGearNum {
    return ([super getFieldUINT8ZValueForField:9 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setFrontGearNum:(FITUInt8z)frontGearNum {
    [super setFieldUINT8ZValueForField:9 andValue:(frontGearNum) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// FrontGear 
- (BOOL)isFrontGearValid {
	const fit::Field* field = [super getField:10];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt8z)getFrontGear {
    return ([super getFieldUINT8ZValueForField:10 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setFrontGear:(FITUInt8z)frontGear {
    [super setFieldUINT8ZValueForField:10 andValue:(frontGear) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// RearGearNum 
- (BOOL)isRearGearNumValid {
	const fit::Field* field = [super getField:11];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt8z)getRearGearNum {
    return ([super getFieldUINT8ZValueForField:11 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setRearGearNum:(FITUInt8z)rearGearNum {
    [super setFieldUINT8ZValueForField:11 andValue:(rearGearNum) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// RearGear 
- (BOOL)isRearGearValid {
	const fit::Field* field = [super getField:12];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt8z)getRearGear {
    return ([super getFieldUINT8ZValueForField:12 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setRearGear:(FITUInt8z)rearGear {
    [super setFieldUINT8ZValueForField:12 andValue:(rearGear) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// DeviceIndex 
- (BOOL)isDeviceIndexValid {
	const fit::Field* field = [super getField:13];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITDeviceIndex)getDeviceIndex {
    return ([super getFieldUINT8ValueForField:13 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setDeviceIndex:(FITDeviceIndex)deviceIndex {
    [super setFieldUINT8ValueForField:13 andValue:(deviceIndex) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// ActivityType 
- (BOOL)isActivityTypeValid {
	const fit::Field* field = [super getField:14];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITActivityType)getActivityType {
    return ([super getFieldENUMValueForField:14 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setActivityType:(FITActivityType)activityType {
    [super setFieldENUMValueForField:14 andValue:(activityType) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// StartTimestamp 
- (BOOL)isStartTimestampValid {
	const fit::Field* field = [super getField:15];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITDate *)getStartTimestamp {
    return FITDateFromTimestamp([super getFieldUINT32ValueForField:15 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setStartTimestamp:(FITDate *)startTimestamp {
    [super setFieldUINT32ValueForField:15 andValue:TimestampFromFITDate(startTimestamp) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 
// StartTimestamp - Sub Fields
// AutoActivityDetectStartTimestamp - StartTimestamp Field - Sub Field 
- (BOOL)isAutoActivityDetectStartTimestampValid
{
    const fit::Field* field = [super getField:15];
    if( FIT_NULL == field ) {
        return FIT_FALSE;
    }

    if(![super canField:15 supportSubField:(FITUInt16)FITEventMesgStartTimestampFieldAutoActivityDetectStartTimestampSubField]) {
        return FIT_FALSE;
    }

    return field->IsValueValid(0, FITEventMesgStartTimestampFieldAutoActivityDetectStartTimestampSubField) == FIT_TRUE ? TRUE : FALSE;
}

- (FITDate *)getAutoActivityDetectStartTimestamp
{
    return FITDateFromTimestamp([super getFieldUINT32ValueForField:15 forIndex:0 andSubFieldIndex:FITEventMesgStartTimestampFieldAutoActivityDetectStartTimestampSubField]);
}

- (void)setAutoActivityDetectStartTimestamp:(FITDate *)autoActivityDetectStartTimestamp
{
    [super setFieldUINT32ValueForField:15 andValue:TimestampFromFITDate(autoActivityDetectStartTimestamp) forIndex:0 andSubFieldIndex:FITEventMesgStartTimestampFieldAutoActivityDetectStartTimestampSubField];
} 

// RadarThreatLevelMax 
- (BOOL)isRadarThreatLevelMaxValid {
	const fit::Field* field = [super getField:21];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITRadarThreatLevelType)getRadarThreatLevelMax {
    return ([super getFieldENUMValueForField:21 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setRadarThreatLevelMax:(FITRadarThreatLevelType)radarThreatLevelMax {
    [super setFieldENUMValueForField:21 andValue:(radarThreatLevelMax) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// RadarThreatCount 
- (BOOL)isRadarThreatCountValid {
	const fit::Field* field = [super getField:22];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITUInt8)getRadarThreatCount {
    return ([super getFieldUINT8ValueForField:22 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setRadarThreatCount:(FITUInt8)radarThreatCount {
    [super setFieldUINT8ValueForField:22 andValue:(radarThreatCount) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// RadarThreatAvgApproachSpeed 
- (BOOL)isRadarThreatAvgApproachSpeedValid {
	const fit::Field* field = [super getField:23];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getRadarThreatAvgApproachSpeed {
    return ([super getFieldFLOAT32ValueForField:23 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setRadarThreatAvgApproachSpeed:(FITFloat32)radarThreatAvgApproachSpeed {
    [super setFieldFLOAT32ValueForField:23 andValue:(radarThreatAvgApproachSpeed) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

// RadarThreatMaxApproachSpeed 
- (BOOL)isRadarThreatMaxApproachSpeedValid {
	const fit::Field* field = [super getField:24];
	if( FIT_NULL == field ) {
		return FALSE;
	}

	return field->IsValueValid() == FIT_TRUE ? TRUE : FALSE;
}

- (FITFloat32)getRadarThreatMaxApproachSpeed {
    return ([super getFieldFLOAT32ValueForField:24 forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD]);
}

- (void)setRadarThreatMaxApproachSpeed:(FITFloat32)radarThreatMaxApproachSpeed {
    [super setFieldFLOAT32ValueForField:24 andValue:(radarThreatMaxApproachSpeed) forIndex:0 andSubFieldIndex:FIT_SUBFIELD_INDEX_MAIN_FIELD];
} 

@end
