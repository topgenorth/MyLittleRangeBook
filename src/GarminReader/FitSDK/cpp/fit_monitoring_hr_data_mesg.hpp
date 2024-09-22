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


#if !defined(FIT_MONITORING_HR_DATA_MESG_HPP)
#define FIT_MONITORING_HR_DATA_MESG_HPP

#include "fit_mesg.hpp"

namespace fit
{

class MonitoringHrDataMesg : public Mesg
{
public:
    class FieldDefNum final
    {
    public:
       static const FIT_UINT8 Timestamp = 253;
       static const FIT_UINT8 RestingHeartRate = 0;
       static const FIT_UINT8 CurrentDayRestingHeartRate = 1;
       static const FIT_UINT8 Invalid = FIT_FIELD_NUM_INVALID;
    };

    MonitoringHrDataMesg(void) : Mesg(Profile::MESG_MONITORING_HR_DATA)
    {
    }

    MonitoringHrDataMesg(const Mesg &mesg) : Mesg(mesg)
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
    // Units: s
    // Comment: Must align to logging interval, for example, time must be 00:00:00 for daily log.
    ///////////////////////////////////////////////////////////////////////
    FIT_DATE_TIME GetTimestamp(void) const
    {
        return GetFieldUINT32Value(253, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set timestamp field
    // Units: s
    // Comment: Must align to logging interval, for example, time must be 00:00:00 for daily log.
    ///////////////////////////////////////////////////////////////////////
    void SetTimestamp(FIT_DATE_TIME timestamp)
    {
        SetFieldUINT32Value(253, timestamp, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of resting_heart_rate field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsRestingHeartRateValid() const
    {
        const Field* field = GetField(0);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns resting_heart_rate field
    // Units: bpm
    // Comment: 7-day rolling average
    ///////////////////////////////////////////////////////////////////////
    FIT_UINT8 GetRestingHeartRate(void) const
    {
        return GetFieldUINT8Value(0, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set resting_heart_rate field
    // Units: bpm
    // Comment: 7-day rolling average
    ///////////////////////////////////////////////////////////////////////
    void SetRestingHeartRate(FIT_UINT8 restingHeartRate)
    {
        SetFieldUINT8Value(0, restingHeartRate, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of current_day_resting_heart_rate field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsCurrentDayRestingHeartRateValid() const
    {
        const Field* field = GetField(1);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns current_day_resting_heart_rate field
    // Units: bpm
    // Comment: RHR for today only. (Feeds into 7-day average)
    ///////////////////////////////////////////////////////////////////////
    FIT_UINT8 GetCurrentDayRestingHeartRate(void) const
    {
        return GetFieldUINT8Value(1, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set current_day_resting_heart_rate field
    // Units: bpm
    // Comment: RHR for today only. (Feeds into 7-day average)
    ///////////////////////////////////////////////////////////////////////
    void SetCurrentDayRestingHeartRate(FIT_UINT8 currentDayRestingHeartRate)
    {
        SetFieldUINT8Value(1, currentDayRestingHeartRate, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

};

} // namespace fit

#endif // !defined(FIT_MONITORING_HR_DATA_MESG_HPP)
