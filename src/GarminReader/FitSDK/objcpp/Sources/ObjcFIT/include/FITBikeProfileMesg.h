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

#import "FITMessage.h"
#import "FITTypes.h"

NS_ASSUME_NONNULL_BEGIN

@interface FITBikeProfileMesg : FITMessage
- (id)init;
// MessageIndex 
- (BOOL)isMessageIndexValid;
- (FITMessageIndex)getMessageIndex;
- (void)setMessageIndex:(FITMessageIndex)messageIndex;
// Name 
- (BOOL)isNameValid;
- (NSString *)getName;
- (void)setName:(NSString *)name;
// Sport 
- (BOOL)isSportValid;
- (FITSport)getSport;
- (void)setSport:(FITSport)sport;
// SubSport 
- (BOOL)isSubSportValid;
- (FITSubSport)getSubSport;
- (void)setSubSport:(FITSubSport)subSport;
// Odometer 
- (BOOL)isOdometerValid;
- (FITFloat32)getOdometer;
- (void)setOdometer:(FITFloat32)odometer;
// BikeSpdAntId 
- (BOOL)isBikeSpdAntIdValid;
- (FITUInt16z)getBikeSpdAntId;
- (void)setBikeSpdAntId:(FITUInt16z)bikeSpdAntId;
// BikeCadAntId 
- (BOOL)isBikeCadAntIdValid;
- (FITUInt16z)getBikeCadAntId;
- (void)setBikeCadAntId:(FITUInt16z)bikeCadAntId;
// BikeSpdcadAntId 
- (BOOL)isBikeSpdcadAntIdValid;
- (FITUInt16z)getBikeSpdcadAntId;
- (void)setBikeSpdcadAntId:(FITUInt16z)bikeSpdcadAntId;
// BikePowerAntId 
- (BOOL)isBikePowerAntIdValid;
- (FITUInt16z)getBikePowerAntId;
- (void)setBikePowerAntId:(FITUInt16z)bikePowerAntId;
// CustomWheelsize 
- (BOOL)isCustomWheelsizeValid;
- (FITFloat32)getCustomWheelsize;
- (void)setCustomWheelsize:(FITFloat32)customWheelsize;
// AutoWheelsize 
- (BOOL)isAutoWheelsizeValid;
- (FITFloat32)getAutoWheelsize;
- (void)setAutoWheelsize:(FITFloat32)autoWheelsize;
// BikeWeight 
- (BOOL)isBikeWeightValid;
- (FITFloat32)getBikeWeight;
- (void)setBikeWeight:(FITFloat32)bikeWeight;
// PowerCalFactor 
- (BOOL)isPowerCalFactorValid;
- (FITFloat32)getPowerCalFactor;
- (void)setPowerCalFactor:(FITFloat32)powerCalFactor;
// AutoWheelCal 
- (BOOL)isAutoWheelCalValid;
- (FITBool)getAutoWheelCal;
- (void)setAutoWheelCal:(FITBool)autoWheelCal;
// AutoPowerZero 
- (BOOL)isAutoPowerZeroValid;
- (FITBool)getAutoPowerZero;
- (void)setAutoPowerZero:(FITBool)autoPowerZero;
// Id 
- (BOOL)isIdValid;
- (FITUInt8)getId;
- (void)setId:(FITUInt8)id;
// SpdEnabled 
- (BOOL)isSpdEnabledValid;
- (FITBool)getSpdEnabled;
- (void)setSpdEnabled:(FITBool)spdEnabled;
// CadEnabled 
- (BOOL)isCadEnabledValid;
- (FITBool)getCadEnabled;
- (void)setCadEnabled:(FITBool)cadEnabled;
// SpdcadEnabled 
- (BOOL)isSpdcadEnabledValid;
- (FITBool)getSpdcadEnabled;
- (void)setSpdcadEnabled:(FITBool)spdcadEnabled;
// PowerEnabled 
- (BOOL)isPowerEnabledValid;
- (FITBool)getPowerEnabled;
- (void)setPowerEnabled:(FITBool)powerEnabled;
// CrankLength 
- (BOOL)isCrankLengthValid;
- (FITFloat32)getCrankLength;
- (void)setCrankLength:(FITFloat32)crankLength;
// Enabled 
- (BOOL)isEnabledValid;
- (FITBool)getEnabled;
- (void)setEnabled:(FITBool)enabled;
// BikeSpdAntIdTransType 
- (BOOL)isBikeSpdAntIdTransTypeValid;
- (FITUInt8z)getBikeSpdAntIdTransType;
- (void)setBikeSpdAntIdTransType:(FITUInt8z)bikeSpdAntIdTransType;
// BikeCadAntIdTransType 
- (BOOL)isBikeCadAntIdTransTypeValid;
- (FITUInt8z)getBikeCadAntIdTransType;
- (void)setBikeCadAntIdTransType:(FITUInt8z)bikeCadAntIdTransType;
// BikeSpdcadAntIdTransType 
- (BOOL)isBikeSpdcadAntIdTransTypeValid;
- (FITUInt8z)getBikeSpdcadAntIdTransType;
- (void)setBikeSpdcadAntIdTransType:(FITUInt8z)bikeSpdcadAntIdTransType;
// BikePowerAntIdTransType 
- (BOOL)isBikePowerAntIdTransTypeValid;
- (FITUInt8z)getBikePowerAntIdTransType;
- (void)setBikePowerAntIdTransType:(FITUInt8z)bikePowerAntIdTransType;
// OdometerRollover 
- (BOOL)isOdometerRolloverValid;
- (FITUInt8)getOdometerRollover;
- (void)setOdometerRollover:(FITUInt8)odometerRollover;
// FrontGearNum 
- (BOOL)isFrontGearNumValid;
- (FITUInt8z)getFrontGearNum;
- (void)setFrontGearNum:(FITUInt8z)frontGearNum;
// FrontGear 
@property(readonly,nonatomic) uint8_t numFrontGearValues;
- (BOOL)isFrontGearValidforIndex : (uint8_t)index;
- (FITUInt8z)getFrontGearforIndex : (uint8_t)index;
- (void)setFrontGear:(FITUInt8z)frontGear forIndex:(uint8_t)index;
// RearGearNum 
- (BOOL)isRearGearNumValid;
- (FITUInt8z)getRearGearNum;
- (void)setRearGearNum:(FITUInt8z)rearGearNum;
// RearGear 
@property(readonly,nonatomic) uint8_t numRearGearValues;
- (BOOL)isRearGearValidforIndex : (uint8_t)index;
- (FITUInt8z)getRearGearforIndex : (uint8_t)index;
- (void)setRearGear:(FITUInt8z)rearGear forIndex:(uint8_t)index;
// ShimanoDi2Enabled 
- (BOOL)isShimanoDi2EnabledValid;
- (FITBool)getShimanoDi2Enabled;
- (void)setShimanoDi2Enabled:(FITBool)shimanoDi2Enabled;

@end

NS_ASSUME_NONNULL_END
