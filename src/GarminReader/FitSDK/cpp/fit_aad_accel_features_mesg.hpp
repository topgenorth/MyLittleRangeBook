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


#if !defined(FIT_AAD_ACCEL_FEATURES_MESG_HPP)
#define FIT_AAD_ACCEL_FEATURES_MESG_HPP

#include "fit_mesg.hpp"

namespace fit
{

class AadAccelFeaturesMesg : public Mesg
{
public:
    class FieldDefNum final
    {
    public:
       static const FIT_UINT8 Timestamp = 253;
       static const FIT_UINT8 Time = 0;
       static const FIT_UINT8 EnergyTotal = 1;
       static const FIT_UINT8 ZeroCrossCnt = 2;
       static const FIT_UINT8 Instance = 3;
       static const FIT_UINT8 TimeAboveThreshold = 4;
       static const FIT_UINT8 Invalid = FIT_FIELD_NUM_INVALID;
    };

    AadAccelFeaturesMesg(void) : Mesg(Profile::MESG_AAD_ACCEL_FEATURES)
    {
    }

    AadAccelFeaturesMesg(const Mesg &mesg) : Mesg(mesg)
    {
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of timestamp field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsTimestampValid() const
    {
        const Field* field = GetField(253);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns timestamp field
    ///////////////////////////////////////////////////////////////////////
    FIT_DATE_TIME GetTimestamp(void) const
    {
        return GetFieldUINT32Value(253, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set timestamp field
    ///////////////////////////////////////////////////////////////////////
    void SetTimestamp(FIT_DATE_TIME timestamp)
    {
        SetFieldUINT32Value(253, timestamp, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of time field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsTimeValid() const
    {
        const Field* field = GetField(0);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns time field
    // Units: s
    // Comment: Time interval length in seconds
    ///////////////////////////////////////////////////////////////////////
    FIT_UINT16 GetTime(void) const
    {
        return GetFieldUINT16Value(0, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set time field
    // Units: s
    // Comment: Time interval length in seconds
    ///////////////////////////////////////////////////////////////////////
    void SetTime(FIT_UINT16 time)
    {
        SetFieldUINT16Value(0, time, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of energy_total field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsEnergyTotalValid() const
    {
        const Field* field = GetField(1);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns energy_total field
    // Comment: Total accelerometer energy in the interval
    ///////////////////////////////////////////////////////////////////////
    FIT_UINT32 GetEnergyTotal(void) const
    {
        return GetFieldUINT32Value(1, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set energy_total field
    // Comment: Total accelerometer energy in the interval
    ///////////////////////////////////////////////////////////////////////
    void SetEnergyTotal(FIT_UINT32 energyTotal)
    {
        SetFieldUINT32Value(1, energyTotal, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of zero_cross_cnt field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsZeroCrossCntValid() const
    {
        const Field* field = GetField(2);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns zero_cross_cnt field
    // Comment: Count of zero crossings
    ///////////////////////////////////////////////////////////////////////
    FIT_UINT16 GetZeroCrossCnt(void) const
    {
        return GetFieldUINT16Value(2, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set zero_cross_cnt field
    // Comment: Count of zero crossings
    ///////////////////////////////////////////////////////////////////////
    void SetZeroCrossCnt(FIT_UINT16 zeroCrossCnt)
    {
        SetFieldUINT16Value(2, zeroCrossCnt, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of instance field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsInstanceValid() const
    {
        const Field* field = GetField(3);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns instance field
    // Comment: Instance ID of zero crossing algorithm
    ///////////////////////////////////////////////////////////////////////
    FIT_UINT8 GetInstance(void) const
    {
        return GetFieldUINT8Value(3, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set instance field
    // Comment: Instance ID of zero crossing algorithm
    ///////////////////////////////////////////////////////////////////////
    void SetInstance(FIT_UINT8 instance)
    {
        SetFieldUINT8Value(3, instance, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of time_above_threshold field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsTimeAboveThresholdValid() const
    {
        const Field* field = GetField(4);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns time_above_threshold field
    // Units: s
    // Comment: Total accelerometer time above threshold in the interval
    ///////////////////////////////////////////////////////////////////////
    FIT_FLOAT32 GetTimeAboveThreshold(void) const
    {
        return GetFieldFLOAT32Value(4, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set time_above_threshold field
    // Units: s
    // Comment: Total accelerometer time above threshold in the interval
    ///////////////////////////////////////////////////////////////////////
    void SetTimeAboveThreshold(FIT_FLOAT32 timeAboveThreshold)
    {
        SetFieldFLOAT32Value(4, timeAboveThreshold, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

};

} // namespace fit

#endif // !defined(FIT_AAD_ACCEL_FEATURES_MESG_HPP)
