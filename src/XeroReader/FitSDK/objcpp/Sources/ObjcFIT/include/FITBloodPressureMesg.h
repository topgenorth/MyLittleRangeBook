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


#import <Foundation/Foundation.h>

#import "FITDate.h"
#import "FITMessage.h"
#import "FITTypes.h"

NS_ASSUME_NONNULL_BEGIN

@interface FITBloodPressureMesg : FITMessage
- (id)init;
// Timestamp 
- (BOOL)isTimestampValid;
- (FITDate *)getTimestamp;
- (void)setTimestamp:(FITDate *)timestamp;
// SystolicPressure 
- (BOOL)isSystolicPressureValid;
- (FITUInt16)getSystolicPressure;
- (void)setSystolicPressure:(FITUInt16)systolicPressure;
// DiastolicPressure 
- (BOOL)isDiastolicPressureValid;
- (FITUInt16)getDiastolicPressure;
- (void)setDiastolicPressure:(FITUInt16)diastolicPressure;
// MeanArterialPressure 
- (BOOL)isMeanArterialPressureValid;
- (FITUInt16)getMeanArterialPressure;
- (void)setMeanArterialPressure:(FITUInt16)meanArterialPressure;
// Map3SampleMean 
- (BOOL)isMap3SampleMeanValid;
- (FITUInt16)getMap3SampleMean;
- (void)setMap3SampleMean:(FITUInt16)map3SampleMean;
// MapMorningValues 
- (BOOL)isMapMorningValuesValid;
- (FITUInt16)getMapMorningValues;
- (void)setMapMorningValues:(FITUInt16)mapMorningValues;
// MapEveningValues 
- (BOOL)isMapEveningValuesValid;
- (FITUInt16)getMapEveningValues;
- (void)setMapEveningValues:(FITUInt16)mapEveningValues;
// HeartRate 
- (BOOL)isHeartRateValid;
- (FITUInt8)getHeartRate;
- (void)setHeartRate:(FITUInt8)heartRate;
// HeartRateType 
- (BOOL)isHeartRateTypeValid;
- (FITHrType)getHeartRateType;
- (void)setHeartRateType:(FITHrType)heartRateType;
// Status 
- (BOOL)isStatusValid;
- (FITBpStatus)getStatus;
- (void)setStatus:(FITBpStatus)status;
// UserProfileIndex 
- (BOOL)isUserProfileIndexValid;
- (FITMessageIndex)getUserProfileIndex;
- (void)setUserProfileIndex:(FITMessageIndex)userProfileIndex;

@end

NS_ASSUME_NONNULL_END
