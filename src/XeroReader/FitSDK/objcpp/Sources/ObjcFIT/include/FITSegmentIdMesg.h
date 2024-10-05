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

@interface FITSegmentIdMesg : FITMessage
- (id)init;
// Name 
- (BOOL)isNameValid;
- (NSString *)getName;
- (void)setName:(NSString *)name;
// Uuid 
- (BOOL)isUuidValid;
- (NSString *)getUuid;
- (void)setUuid:(NSString *)uuid;
// Sport 
- (BOOL)isSportValid;
- (FITSport)getSport;
- (void)setSport:(FITSport)sport;
// Enabled 
- (BOOL)isEnabledValid;
- (FITBool)getEnabled;
- (void)setEnabled:(FITBool)enabled;
// UserProfilePrimaryKey 
- (BOOL)isUserProfilePrimaryKeyValid;
- (FITUInt32)getUserProfilePrimaryKey;
- (void)setUserProfilePrimaryKey:(FITUInt32)userProfilePrimaryKey;
// DeviceId 
- (BOOL)isDeviceIdValid;
- (FITUInt32)getDeviceId;
- (void)setDeviceId:(FITUInt32)deviceId;
// DefaultRaceLeader 
- (BOOL)isDefaultRaceLeaderValid;
- (FITUInt8)getDefaultRaceLeader;
- (void)setDefaultRaceLeader:(FITUInt8)defaultRaceLeader;
// DeleteStatus 
- (BOOL)isDeleteStatusValid;
- (FITSegmentDeleteStatus)getDeleteStatus;
- (void)setDeleteStatus:(FITSegmentDeleteStatus)deleteStatus;
// SelectionType 
- (BOOL)isSelectionTypeValid;
- (FITSegmentSelectionType)getSelectionType;
- (void)setSelectionType:(FITSegmentSelectionType)selectionType;

@end

NS_ASSUME_NONNULL_END
